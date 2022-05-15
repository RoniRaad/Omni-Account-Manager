using AccountManager.Core.Enums;
using AccountManager.Core.Factories;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Models.AppSettings;
using AccountManager.Core.Models.RiotGames.League;
using AccountManager.Core.Models.RiotGames.League.Requests;
using AccountManager.Core.Models.RiotGames.League.Responses;
using AccountManager.Core.Models.RiotGames.Requests;
using AccountManager.Core.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace AccountManager.Infrastructure.Clients
{
    public class RemoteLeagueClient : ILeagueClient
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ITokenService _leagueTokenService;
        private readonly LocalLeagueClient _localLeagueClient;
        private readonly AlertService _alertService;
        private readonly IUserSettingsService<UserSettings> _settings;
        private readonly IDistributedCache _persistantCache;
        private readonly IRiotClient _riotClient;
        private readonly RiotApiUri _riotApiUri;

        public RemoteLeagueClient(IMemoryCache memoryCache, IHttpClientFactory httpClientFactory, GenericFactory<AccountType, ITokenService> tokenFactory,
            LocalLeagueClient localLeagueClient, IUserSettingsService<UserSettings> settings, AlertService alertService, IDistributedCache persistantCache,
            IRiotClient riotClient, IOptions<RiotApiUri> riotApiOptions)
        {
            _memoryCache = memoryCache;
            _leagueTokenService = tokenFactory.CreateImplementation(AccountType.League);
            _httpClientFactory = httpClientFactory;
            _localLeagueClient = localLeagueClient;
            _httpClientFactory = httpClientFactory;
            _settings = settings;
            _alertService = alertService;
            _persistantCache = persistantCache;
            _riotClient = riotClient;
            _riotApiUri = riotApiOptions.Value;
        }

        public async Task<string> GetRankByUsernameAsync(string username)
        {
            if (!_leagueTokenService.TryGetPortAndToken(out string token, out string port))
                return "";
            var client = _httpClientFactory.CreateClient("CloudflareBypass");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"riot:{token}")));
            var response = await client.GetAsync($"https://127.0.0.1:{port}/lol-summoner/v1/summoners?name={username}");
            var summoner = await response.Content.ReadFromJsonAsync<LeagueAccount>();
            var rankResponse = await client.GetAsync($"https://127.0.0.1:{port}/lol-ranked/v1/ranked-stats/{summoner?.Puuid}");
            var summonerRanking = await rankResponse.Content.ReadFromJsonAsync<LeagueSummonerRank>();
            return $"{summonerRanking?.QueueMap?.RankedSoloDuoStats?.Tier} {summonerRanking?.QueueMap?.RankedSoloDuoStats?.Division}";
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
                Tier = rankedStats?.Tier,
                Ranking = rankedStats?.Rank,
            };
        }

        public async Task<List<Queue>> GetRankQueuesByPuuidAsync(Account account)
        {
            var sessionToken = await GetLeagueSessionToken(account);

            var client = _httpClientFactory.CreateClient("CloudflareBypass");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessionToken);
            var rankResponse = await client.GetFromJsonAsync<LeagueRankedResponse>($"{_riotApiUri.LeagueNA}/leagues-ledge/v2/rankedStats/puuid/{account.PlatformId}");

            if (rankResponse?.Queues is null)
                return new List<Queue>();

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
            if (rankedStats?.Tier?.ToLower() == "none" || rankedStats?.Tier is null)
                return new Rank()
                {
                    Tier = "UNRANKED",
                    Ranking = ""
                };

            return new Rank()
            {
                Tier = rankedStats?.Tier,
                Ranking = rankedStats?.Rank,
            };
        }

        private async Task<string> GetRiotAuthToken(Account account)
        {
            var request = new RiotSessionRequest
            {
                Id = "lol",
                Nonce = "1",
                RedirectUri = "http://localhost/redirect",
                ResponseType = "token id_token",
                Scope = "openid link ban lol_region"
            };

            var response = await _riotClient.RiotAuthenticate(request, account);

            if (response is null || response?.Content?.Response?.Parameters?.Uri is null)
                return string.Empty;

            var matches = Regex.Matches(response.Content.Response.Parameters.Uri,
                @"access_token=((?:[a-zA-Z]|\d|\.|-|_)*).*id_token=((?:[a-zA-Z]|\d|\.|-|_)*).*expires_in=(\d*)");

            var token = matches[0].Groups[1].Value;

            return token;
        }

        private async Task<string> GetUserInfo(string riotToken)
        {
            string userInfo;
            var client = _httpClientFactory.CreateClient("CloudflareBypass");

            client.DefaultRequestHeaders.Authorization = new("Bearer", riotToken);
            var userInfoResponse = await client.GetAsync($"{_riotApiUri.Auth}/userinfo");
            userInfoResponse.EnsureSuccessStatusCode();
            userInfo = await userInfoResponse.Content.ReadAsStringAsync();

            return userInfo;
        }

        private async Task<string> GetEntitlementJWT(string riotToken)
        {
            string entitlement;
            var client = _httpClientFactory.CreateClient("CloudflareBypass");

            client.DefaultRequestHeaders.Remove("Authorization");
            client.DefaultRequestHeaders.Authorization = new("Bearer", riotToken);

            var entitlementResponse = await client.PostAsJsonAsync($"{_riotApiUri.Entitlement}/api/token/v1", new { urn = "urn:entitlement" });
            entitlementResponse.EnsureSuccessStatusCode();
            var entitlementResponseDeserialized = await entitlementResponse.Content.ReadFromJsonAsync<EntitlementResponse>();
            if (entitlementResponseDeserialized?.EntitlementToken is null)
                return string.Empty;

            entitlement = entitlementResponseDeserialized.EntitlementToken;

            return entitlement;
        }

        private async Task<string> GetLeagueLoginToken(Account account)
        {
            string token;
            var riotToken = await GetRiotAuthToken(account);
            var userInfo = await GetUserInfo(riotToken);
            var entitlement = await GetEntitlementJWT(riotToken);
            if (string.IsNullOrEmpty(riotToken) 
                || string.IsNullOrEmpty(userInfo)
                || string.IsNullOrEmpty(entitlement))
                return string.Empty;

            var client = _httpClientFactory.CreateClient("CloudflareBypass");

            client.DefaultRequestHeaders.Authorization = new("Bearer", riotToken);

            var loginResponse = await client.PostAsJsonAsync($"{_riotApiUri.LeagueSessionUS}/login-queue/v2/login/products/lol/regions/na1", new LoginRequest
            {
                Entitlements = entitlement,
                UserInfo = userInfo
            });
            loginResponse.EnsureSuccessStatusCode();
            var tokenResponse = await loginResponse.Content.ReadFromJsonAsync<TokenResponse>();
            if (tokenResponse?.Token is null)
                return string.Empty;

            token = tokenResponse.Token;

            return token;
        }

        public async Task<string> CreateLeagueSession(Account account)
        {
            string sessionToken;

            var puuId = account.PlatformId;
            if (puuId is null)
                return string.Empty;

            var leagueToken = await GetLeagueLoginToken(account);
            if (string.IsNullOrEmpty(leagueToken))
                return string.Empty;

            var client = _httpClientFactory.CreateClient("CloudflareBypass");

            client.DefaultRequestHeaders.Authorization = new("Bearer", leagueToken);

            var sessionResponse = await client.PostAsJsonAsync($"{_riotApiUri.LeagueSessionUS}/session-external/v1/session/create", new PostSessionsRequest
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
            if (_memoryCache.TryGetValue<string>("GetLeagueSessionToken", out string sessionToken) && await TestLeagueToken(sessionToken))
                return sessionToken;

            sessionToken = await _localLeagueClient.GetLocalSessionToken();
            if (string.IsNullOrEmpty(sessionToken) || !await TestLeagueToken(sessionToken))
                sessionToken = await CreateLeagueSession(account);

            if (!string.IsNullOrEmpty(sessionToken))
                _memoryCache.Set("GetLeagueSessionToken", sessionToken);

            return sessionToken;
        }

        public async Task<MatchHistoryRequest?> GetUserMatchHistory(Account account, int startIndex, int endIndex)
        {
            var token = await GetLeagueSessionToken(account);
            var client = _httpClientFactory.CreateClient("CloudflareBypass");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var rankResponse = await client.GetFromJsonAsync<MatchHistoryRequest>($"{_riotApiUri.LeagueSessionUS}/match-history-query/v1/products/lol/player/{account.PlatformId}/SUMMARY?startIndex={startIndex}&count={endIndex}");
            
            return rankResponse;
        }

        public async Task<bool> TestLeagueToken(string token)
        {
            var client = _httpClientFactory.CreateClient("CloudflareBypass");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var rankResponse = await client.GetAsync($"{_riotApiUri.LeagueNA}/leagues-ledge/v2/rankedStats/puuid/fakepuuid");
            return rankResponse.IsSuccessStatusCode;
        }
    }
}
