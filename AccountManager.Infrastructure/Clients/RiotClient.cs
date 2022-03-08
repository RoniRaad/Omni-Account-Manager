using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Models.RiotGames.Valorant;
using AccountManager.Core.Models.RiotGames.Valorant.Responses;
using CloudFlareUtilities;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace AccountManager.Infrastructure.Clients
{
    public partial class RiotClient : IRiotClient
    {
        private string bearerToken = "";
        private string entitlementToken = "";
        private readonly HttpClient _httpClient;

        public RiotClient(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("CloudflareBypass");

        }

        private async Task AddHeadersToClient()
        {
            if (_httpClient.DefaultRequestHeaders.Contains("X-Riot-ClientVersion"))
                return;
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Riot-ClientPlatform", "ew0KCSJwbGF0Zm9ybVR5cGUiOiAiUEMiLA0KCSJwbGF0Zm9ybU9TIjogIldpbmRvd3MiLA0KCSJwbGF0Zm9ybU9TVmVyc2lvbiI6ICIxMC4wLjE5MDQyLjEuMjU2LjY0Yml0IiwNCgkicGxhdGZvcm1DaGlwc2V0IjogIlVua25vd24iDQp9");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Riot-ClientVersion", await GetExpectedClientVersion());
        }

        public async Task<string> GetExpectedClientVersion()
        {
            var response = await _httpClient.GetFromJsonAsync<ExpectedClientVersionResponse>("https://valorant-api.com/v1/version");
            return response.Data.RiotClientVersion;
        }
        public async Task<string> GetToken(string username, string pass)
        {
            await AddHeadersToClient();

            await _httpClient.PostAsJsonAsync("https://auth.riotgames.com/api/v1/authorization", new AuthRequestPostResponse {
                Id = "play-valorant-web-prod",
                Nonce = "1",
                RedirectUri = "https://playvalorant.com/opt_in",
                ResponseType = "token id_token"
            });

            var authResponse = await _httpClient.PutAsJsonAsync("https://auth.riotgames.com/api/v1/authorization", new AuthRequest
            {
                Type = "auth",
                Username = username,
                Password = pass
            });

            var authResponseDeserialized = await authResponse.Content.ReadFromJsonAsync<TokenResponseWrapper>();
            var matches = Regex.Matches(authResponseDeserialized.Response.Parameters.Uri, @"access_token=((?:[a-zA-Z]|\d|\.|-|_)*).*id_token=((?:[a-zA-Z]|\d|\.|-|_)*).*expires_in=(\d*)");
            var token = matches[0].Groups[1].Value;

            return token;
        }
        public async Task<string> GetEntitlementToken(string token)
        {
            await AddHeadersToClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var entitlementResponse = await _httpClient.PostAsJsonAsync("https://entitlements.auth.riotgames.com/api/token/v1", new { });
            var entitlementResponseDeserialized = await entitlementResponse.Content.ReadFromJsonAsync<EntitlementTokenResponse>();

            return entitlementResponseDeserialized.EntitlementToken;
        }
        public async Task GetAuth(string username, string password)
        {
            bearerToken = await GetToken(username, password);
            entitlementToken = await GetEntitlementToken(bearerToken);
        }
        public async Task<string> GetPuuId(string username, string password)
        {
            await GetAuth(username, password);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Riot-Entitlements-JWT", entitlementToken);

            var response = await _httpClient.GetFromJsonAsync<UserInfoResponse>("https://auth.riotgames.com/userinfo");
            return response.PuuId;
        }
        public async Task<Rank> GetValorantRank(Account account)
        {
            int rankNumber;
            await GetAuth(account.Username, account.Password);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Riot-Entitlements-JWT", entitlementToken);

            var response = await _httpClient.GetFromJsonAsync<ValorantRankedResponse>($"https://pd.na.a.pvp.net/mmr/v1/players/{account.Id}");

            if (response?.QueueSkills?.Competitive?.TotalGamesNeededForRating > 0)
                return new Rank() {
                    Tier = "PLACEMENTS",
                    Ranking = $"{5 - response?.QueueSkills?.Competitive?.TotalGamesNeededForRating}/5"
                };
            else if (response?.QueueSkills?.Competitive?.CurrentSeasonGamesNeededForRating > 0)
                return new Rank()
                {
                    Tier = "PLACEMENTS",
                    Ranking = $"{1 - response?.QueueSkills?.Competitive?.CurrentSeasonGamesNeededForRating}/1"
                };
            else
                rankNumber = response.LatestCompetitiveUpdate.TierAfterUpdate.Value;

            var valorantRanking = new List<string>() {
                "IRON",
                "BRONZE",
                "SILVER" ,
                "GOLD" ,
                "PLATINUM" ,
                "DIAMOND" ,
                "IMMORTAL" ,
            };

            var rank = new Rank()
            {
                Tier = valorantRanking[rankNumber / 3],
                Ranking = new string('I', rankNumber % 3 + 1)
            };

            return rank;
        }
    }
}
