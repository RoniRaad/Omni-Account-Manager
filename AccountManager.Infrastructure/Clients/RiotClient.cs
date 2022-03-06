using AccountManager.Core.Models;
using CloudFlareUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AccountManager.Infrastructure.Clients
{
    public class RiotClient : IRiotClient
    {
        private string bearerToken = "";
        private string entitlementToken = "";
        private HttpClient GenerateClient()
        {
            var handler = new ClearanceHandler
            {
                MaxRetries = 2
            };

            var client = new HttpClient(handler);
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-Riot-ClientPlatform", "ew0KCSJwbGF0Zm9ybVR5cGUiOiAiUEMiLA0KCSJwbGF0Zm9ybU9TIjogIldpbmRvd3MiLA0KCSJwbGF0Zm9ybU9TVmVyc2lvbiI6ICIxMC4wLjE5MDQyLjEuMjU2LjY0Yml0IiwNCgkicGxhdGZvcm1DaGlwc2V0IjogIlVua25vd24iDQp9");
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-Riot-ClientVersion", "release-04.03-shipping-6-671292");

            return client;
        }
        public async Task<string> GetToken(string username, string pass)
        {
            var client = GenerateClient();

            await client.PostAsJsonAsync("https://auth.riotgames.com/api/v1/authorization", new AuthRequestPostResponse {
                Id = "play-valorant-web-prod",
                Nonce = "1",
                RedirectUri = "https://playvalorant.com/opt_in",
                ResponseType = "token id_token"
            });

            var authResponse = await client.PutAsJsonAsync("https://auth.riotgames.com/api/v1/authorization", new AuthRequest
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
            var client = GenerateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var entitlementResponse = await client.PostAsJsonAsync("https://entitlements.auth.riotgames.com/api/token/v1", new { });
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
            var client = GenerateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-Riot-Entitlements-JWT", entitlementToken);

            var response = await client.GetFromJsonAsync<UserInfoResponse>("https://auth.riotgames.com/userinfo");
            return response.PuuId;
        }
        public async Task<Rank> GetValorantRank(Account account)
        {
            int rankNumber;
            await GetAuth(account.Username, account.Password);
            var client = GenerateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-Riot-Entitlements-JWT", entitlementToken);

            var response = await client.GetFromJsonAsync<ValorantRankedResponse>($"https://pd.na.a.pvp.net/mmr/v1/players/{account.Id}");
            var responses = await client.GetAsync($"https://pd.na.a.pvp.net/mmr/v1/players/{account.Id}");

            if (response?.QueueSkills?.Competitive?.TotalGamesNeededForRating > 0)
                return new Rank() {
                    Tier = "PLACEMENTS",
                    Ranking = $"{response?.QueueSkills?.Competitive?.TotalGamesNeededForRating - 5}/5"
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
internal class AuthRequestPostResponse
{
    [JsonPropertyName("client_id")]
    public string Id { get; set; }
    [JsonPropertyName("nonce")]
    public string Nonce { get; set; }
    [JsonPropertyName("redirect_uri")]
    public string RedirectUri { get; set; }
    [JsonPropertyName("response_type")]
    public string ResponseType { get; set; }
}

internal class AuthRequest
{
    [JsonPropertyName("type")]
    public string Type { get; set; }
    [JsonPropertyName("username")]
    public string Username { get; set; }
    [JsonPropertyName("password")]
    public string Password { get; set; }
}
internal class TokenResponseWrapper
{
    [JsonPropertyName("response")]
    public TokenResponse Response { get; set; }
}
internal class TokenResponse
{
    [JsonPropertyName("parameters")]
    public TokenParameters Parameters { get; set; }
}
internal class TokenParameters
{
    [JsonPropertyName("uri")]
    public string Uri { get; set; }
}
internal class EntitlementTokenResponse
{
    [JsonPropertyName("entitlements_token")]
    public string EntitlementToken { get; set; }
}
internal class UserInfoResponse
{
    [JsonPropertyName("sub")]
    public string PuuId { get; set; }
}
internal class ValorantRankedResponse
{
    public ValorantCompetitiveUpdate? LatestCompetitiveUpdate { get; set; }
    public ValorantQueueSkills? QueueSkills { get; set; }
}
internal class ValorantCompetitiveUpdate
{
    public int? TierAfterUpdate { get; set; }
}
internal class ValorantQueueSkills
{
    [JsonPropertyName("competitive")]
    public ValorantCompetitive? Competitive { get; set; }
}
internal class ValorantCompetitive
{
    public int? TotalGamesNeededForRating { get; set; }
}