using AccountManager.Core.Enums;
using AccountManager.Core.Factories;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Models.AppSettings;
using AccountManager.Core.Models.RiotGames.League;
using AccountManager.Core.Models.RiotGames.League.Requests;
using AccountManager.Core.Models.RiotGames.League.Responses;
using AccountManager.Core.Models.RiotGames.Requests;
using AccountManager.Core.Models.RiotGames.TeamFightTactics.Responses;
using AccountManager.Core.Services;
using AutoMapper;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace AccountManager.Infrastructure.Clients
{
    public class RemoteLeagueClient : ILeagueClient
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly LocalLeagueClient _localLeagueClient;
        private readonly IUserSettingsService<UserSettings> _settings;
        private readonly IRiotClient _riotClient;
        private readonly RiotApiUri _riotApiUri;
        private readonly IMapper _autoMapper;
        private readonly ICurlRequestBuilder _curlRequestBuilder;
        private static SemaphoreSlim semaphore = new(1, 1);

        public RemoteLeagueClient(IMemoryCache memoryCache, IHttpClientFactory httpClientFactory,
            LocalLeagueClient localLeagueClient, IUserSettingsService<UserSettings> settings,
            IRiotClient riotClient, IOptions<RiotApiUri> riotApiOptions, IMapper autoMapper,
            ICurlRequestBuilder curlRequestBuilder)
        {
            _memoryCache = memoryCache;
            _httpClientFactory = httpClientFactory;
            _localLeagueClient = localLeagueClient;
            _httpClientFactory = httpClientFactory;
            _settings = settings;
            _riotClient = riotClient;
            _riotApiUri = riotApiOptions.Value;
            _autoMapper = autoMapper;
            _curlRequestBuilder = curlRequestBuilder;
        }

        public async Task<Rank> GetSummonerRankByPuuidAsync(Account account)
        {

            if (!_settings.Settings.UseAccountCredentials)
                return new Rank();

            var queue = await GetRankQueuesByPuuidAsync(account);
            var rankedStats = queue.Find((match) => match.QueueType == "RANKED_SOLO_5x5");

            return _autoMapper.Map<LeagueRank>(new Rank()
            {
                Tier = rankedStats?.Tier,
                Ranking = rankedStats?.Rank,
            });
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

            if (!_settings.Settings.UseAccountCredentials)
                return new Rank();

            var queue = await GetRankQueuesByPuuidAsync(account);
            var rankedStats = queue.Find((match) => match.QueueType == "RANKED_TFT");
            if (rankedStats?.Tier?.ToLower() == "none" || rankedStats?.Tier is null)
                return _autoMapper.Map<TeamFightTacticsRank>(new Rank()
                {
                    Tier = "UNRANKED",
                    Ranking = ""
                });

            return _autoMapper.Map<TeamFightTacticsRank>(new Rank()
            {
                Tier = rankedStats?.Tier,
                Ranking = rankedStats?.Rank,
            });
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
            var response = await _curlRequestBuilder.CreateBuilder()
            .SetUri($"{_riotApiUri.Auth}/userinfo")
            .SetBearerToken(riotToken)
            .SetUserAgent("RiotClient/50.0.0.4396195.4381201 rso-auth (Windows;10;;Professional, x64)")
            .Get();

            return response?.ResponseContent ?? "";
        }

        private async Task<string> GetEntitlementJWT(string riotToken)
        {
            string entitlement;
            var response = await _curlRequestBuilder.CreateBuilder()
            .SetUri($"{_riotApiUri.Entitlement}/api/token/v1")
            .SetContent(new { urn = "urn:entitlement" })
            .SetBearerToken(riotToken)
            .SetUserAgent("RiotClient/50.0.0.4396195.4381201 rso-auth (Windows;10;;Professional, x64)")
            .Post<EntitlementResponse>();

            entitlement = response?.ResponseContent?.EntitlementToken ?? "";

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
            client.DefaultRequestVersion = HttpVersion.Version20;

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
            client.DefaultRequestVersion = HttpVersion.Version20;

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
            await semaphore.WaitAsync();
            if (_memoryCache.TryGetValue<string>("GetLeagueSessionToken", out string? sessionToken)
                && sessionToken is not null
                && await TestLeagueToken(sessionToken))
                return sessionToken;

            sessionToken = await _localLeagueClient.GetLocalSessionToken();
            if (string.IsNullOrEmpty(sessionToken) || !await TestLeagueToken(sessionToken))
                sessionToken = await CreateLeagueSession(account);

            if (!string.IsNullOrEmpty(sessionToken))
                _memoryCache.Set("GetLeagueSessionToken", sessionToken);

            semaphore.Release(1);
            return sessionToken;
        }

        public async Task<List<LeagueQueueMapResponse>?> GetLeagueQueueMappings()
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetFromJsonAsync<List<LeagueQueueMapResponse>>($"{_riotApiUri.RiotCDN}/docs/lol/queues.json");

            return response;
        }

        private async Task<MatchHistoryResponse?> GetLeagueMatchHistory(Account account, int startIndex, int endIndex)
        {
            if (!_settings.Settings.UseAccountCredentials)
                return new();

            var token = await GetLeagueSessionToken(account);
            var client = _httpClientFactory.CreateClient("CloudflareBypass");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var rankResponse = await client.GetFromJsonAsync<MatchHistoryResponse>($"{_riotApiUri.LeagueSessionUS}/match-history-query/v1/products/lol/player/{account.PlatformId}/SUMMARY?startIndex={startIndex}&count={endIndex}");
            return rankResponse;
        }

        public async Task<UserChampSelectHistory> GetUserChampSelectHistory(Account account, int startIndex, int endIndex)
        {
            var rankResponse = await GetUserLeagueMatchHistory(account, startIndex, endIndex);
            var userInGames = rankResponse?.Games?.Select((game) => game?.Json?.Participants?.FirstOrDefault((participants) => participants?.Puuid == account.PlatformId, null));
            var selectedChampGroup = userInGames?.GroupBy((userInGame) => userInGame?.ChampionName);
            return new UserChampSelectHistory()
            {
                Champs = selectedChampGroup.Select((grouping) => new ChampSelectedCount()
                {
                    ChampName = grouping?.Key,
                    SelectedCount = grouping.Count()
                })
            };
        }

        public async Task<MatchHistory?> GetUserLeagueMatchHistory(Account account, int startIndex, int endIndex)
        {
            
            var rankResponse = await GetLeagueMatchHistory(account, startIndex, endIndex);

            if (rankResponse is null)
                return null;

            var matchHistory = _autoMapper.Map<MatchHistory>(rankResponse);
            return matchHistory;
        }

        public async Task<TeamFightTacticsMatchHistory?> GetUserTeamFightTacticsMatchHistory(Account account, int startIndex, int endIndex)
        {

            if (!_settings.Settings.UseAccountCredentials)
                return new();

            var token = await GetLeagueSessionToken(account);
            var client = _httpClientFactory.CreateClient("CloudflareBypass");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var rankResponse = await client.GetFromJsonAsync<TeamFightTacticsMatchHistory>($"{_riotApiUri.LeagueSessionUS}/match-history-query/v1/products/tft/player/{account.PlatformId}/SUMMARY?startIndex={startIndex}&count={endIndex}");

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
