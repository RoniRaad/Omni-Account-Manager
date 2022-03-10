using AccountManager.Core.Enums;
using AccountManager.Core.Factories;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Models.RiotGames;
using AccountManager.Core.Models.RiotGames.League;
using AccountManager.Core.Models.RiotGames.League.Requests;
using AccountManager.Core.Models.RiotGames.League.Responses;
using CloudFlareUtilities;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using static AccountManager.Infrastructure.Clients.LocalLeagueClient;

namespace AccountManager.Infrastructure.Clients
{
    public class RemoteLeagueClient : ILeagueClient
    {
        private IMemoryCache _memoryCache;
        private IHttpClientFactory _httpClientFactory;
        private readonly ITokenService _leagueTokenService;
        private readonly LocalLeagueClient _localLeagueClient;
        private readonly HttpClient _httpClient;
        private readonly IUserSettingsService<UserSettings> _settings;
        public RemoteLeagueClient(IMemoryCache memoryCache, IHttpClientFactory httpClientFactory, GenericFactory<AccountType, ITokenService> tokenFactory, LocalLeagueClient localLeagueClient, IUserSettingsService<UserSettings> settings)
        {
            _memoryCache = memoryCache;
            _leagueTokenService = tokenFactory.CreateImplementation(AccountType.League);
            _httpClientFactory = httpClientFactory;
            _localLeagueClient = localLeagueClient;
            _httpClient = httpClientFactory.CreateClient("CloudflareBypass");
            _settings = settings;
        }

        public async Task<string> GetRankByUsernameAsync(string username)
        {
            if (!_leagueTokenService.TryGetPortAndToken(out string token, out string port))
                return "";

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"riot:{token}")));
            var response = await _httpClient.GetAsync($"https://127.0.0.1:{port}/lol-summoner/v1/summoners?name={username}");
            var summoner = await response.Content.ReadFromJsonAsync<LeagueAccount>();
            var rankResponse = await _httpClient.GetAsync($"https://127.0.0.1:{port}/lol-ranked/v1/ranked-stats/{summoner.Puuid}");
            var summonerRanking = await rankResponse.Content.ReadFromJsonAsync<LeagueSummonerRank>();
            return $"{summonerRanking.QueueMap.RankedSoloDuoStats.Tier} {summonerRanking.QueueMap.RankedSoloDuoStats.Division}";
        }
        public async Task<Rank> GetSummonerRankByPuuidAsync(Account account)
        {
            if (_localLeagueClient.IsClientOpen())
                return await _localLeagueClient.GetSummonerRankByPuuidAsync(account);

            if (!_settings.Settings.UseAccountCredentials)
                return new Rank();

            var queue = await GetRankQueuesByPuuidAsync(account);
            var rankedStats = queue.Find((match) => match.QueueType == "RANKED_SOLO_5x5");

            return new Rank()
            {
                Tier = rankedStats.Tier,
                Ranking = rankedStats.Rank,
            };
        }

        public async Task<List<Queue>> GetRankQueuesByPuuidAsync(Account account)
        {
            var sessionToken = await GetLeagueSessionToken(account);

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessionToken);
            var rankResponse = await _httpClient.GetFromJsonAsync<LeagueRankedResponse>($"https://na-blue.lol.sgp.pvp.net/leagues-ledge/v2/rankedStats/puuid/{account.Id}");
            var rankedStats = rankResponse.Queues.Find((match) => match.QueueType == "RANKED_SOLO_5x5");

            return rankResponse.Queues;
        }

        public async Task<Rank> GetTFTRankByPuuidAsync(Account account)
        {
            if (_localLeagueClient.IsClientOpen())
                return await _localLeagueClient.GetTFTRankByPuuidAsync(account);

            if (!_settings.Settings.UseAccountCredentials)
                return new Rank();

            var queue = await GetRankQueuesByPuuidAsync(account);
            var rankedStats = queue.Find((match) => match.QueueType == "RANKED_TFT");
            if (rankedStats.Tier.ToLower() == "none")
                return new Rank()
                {
                    Tier = "UNRANKED",
                    Ranking = ""
                };

            return new Rank()
            {
                Tier = rankedStats.Tier,
                Ranking = rankedStats.Rank,
            };
        }

        private async Task<string> GetRiotAuthToken(Account account)
        {
            var handler = new ClearanceHandler
            {
                MaxRetries = 2
            };

            using (var client = new HttpClient(handler))
            {
                string token;
                var initialAuthTokenResponse = await client.PostAsJsonAsync("https://auth.riotgames.com/api/v1/authorization", new InitialAuthTokenRequest
                {
                    Id = "lol",
                    Nonce = "1",
                    RedirectUri = "http://localhost/redirect",
                    ResponseType = "token id_token",
                    Scope = "openid link ban lol_region"
                });
                initialAuthTokenResponse.EnsureSuccessStatusCode();

                var finalAuthRequest = await client.PutAsJsonAsync("https://auth.riotgames.com/api/v1/authorization", new FinalAuthTokenRequest
                {
                    Type = "auth",
                    Username = account.Username,
                    Password = account.Password,
                    Region = null
                });
                finalAuthRequest.EnsureSuccessStatusCode();

                var riotTokenResponse = await finalAuthRequest.Content.ReadFromJsonAsync<RiotAuthTokenWrapper>();

                var matches = Regex.Matches(riotTokenResponse.Response.Parameters.Uri,
                    @"access_token=((?:[a-zA-Z]|\d|\.|-|_)*).*id_token=((?:[a-zA-Z]|\d|\.|-|_)*).*expires_in=(\d*)");
                token = matches[0].Groups[1].Value;
                return token;
            }
        }
        private async Task<string> GetUserInfo(Account account, string riotToken)
        {
            string userInfo;

            _httpClient.DefaultRequestHeaders.Authorization = new("Bearer", riotToken);
            var userInfoResponse = await _httpClient.GetAsync("https://auth.riotgames.com/userinfo");
            userInfoResponse.EnsureSuccessStatusCode();
            userInfo = await userInfoResponse.Content.ReadAsStringAsync();

            return userInfo;
        }
        private async Task<string> GetEntitlementJWT(Account account, string riotToken)
        {
            string entitlement;
            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            _httpClient.DefaultRequestHeaders.Authorization = new("Bearer", riotToken);

            var entitlementResponse = await _httpClient.PostAsJsonAsync("https://entitlements.auth.riotgames.com/api/token/v1", new { urn = "urn:entitlement" });
            entitlementResponse.EnsureSuccessStatusCode();
            var entitlementResponseDeserialized = await entitlementResponse.Content.ReadFromJsonAsync<EntitlementResponse>();
            entitlement = entitlementResponseDeserialized.EntitlementToken;

            return entitlement;
        }
        private async Task<string> GetLeagueLoginToken(Account account)
        {
            string token;
            var riotToken = await GetRiotAuthToken(account);
            var userInfo = await GetUserInfo(account, riotToken);
            var entitlement = await GetEntitlementJWT(account, riotToken);

            _httpClient.DefaultRequestHeaders.Authorization = new("Bearer", riotToken);

            var loginResponse = await _httpClient.PostAsJsonAsync($"https://usw.pp.riotgames.com/login-queue/v2/login/products/lol/regions/na1", new LoginRequest
            {
                Entitlements = entitlement,
                UserInfo = userInfo
            });
            loginResponse.EnsureSuccessStatusCode();
            var tokenResponse = await loginResponse.Content.ReadFromJsonAsync<TokenResponse>();
            token = tokenResponse.Token;

            return token;
        }
        public async Task<string> CreateLeagueSession(Account account)
        {
            string sessionToken;

            var puuId = account.Id;
            var leagueToken = await GetLeagueLoginToken(account);


            _httpClient.DefaultRequestHeaders.Authorization = new("Bearer", leagueToken);

            var sessionResponse = await _httpClient.PostAsJsonAsync($"https://usw.pp.riotgames.com/session-external/v1/session/create", new PostSessionsRequest
            {
                Claims = new Claims
                {
                    CName = "lcu"
                },
                Product = "lol",
                PuuId = puuId,
                Region = "NA1"
            });
            sessionResponse.EnsureSuccessStatusCode();
            sessionToken = await sessionResponse.Content.ReadAsStringAsync();

            return sessionToken.Replace("\"", "");
        }
        public async Task<string> GetLeagueSessionToken(Account account)
        {
            if (_memoryCache.TryGetValue<string>("GetLeagueSessionToken", out string sessionToken))
                if (await TestLeagueToken(sessionToken))
                    return sessionToken;

            sessionToken = await _localLeagueClient.GetLocalSessionToken();
            if (string.IsNullOrEmpty(sessionToken) || !await TestLeagueToken(sessionToken))
                sessionToken = await CreateLeagueSession(account);

            _memoryCache.Set("GetLeagueSessionToken", sessionToken);
            return sessionToken;
        }

        public async Task<bool> TestLeagueToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var rankResponse = await _httpClient.GetAsync($"https://na-blue.lol.sgp.pvp.net/leagues-ledge/v2/rankedStats/puuid/fakepuuid");
            return rankResponse.IsSuccessStatusCode;
        }
    }
}
