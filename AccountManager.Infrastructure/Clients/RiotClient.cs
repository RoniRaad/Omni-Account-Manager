using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Models.RiotGames;
using AccountManager.Core.Models.RiotGames.Valorant;
using AccountManager.Core.Models.RiotGames.Valorant.Responses;
using AccountManager.Core.Services;
using CloudFlareUtilities;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AccountManager.Core.Static;
using System.Text.RegularExpressions;
using AccountManager.Core.Models.RiotGames.Requests;
using AccountManager.Core.Models.AppSettings;
using Microsoft.Extensions.Options;
using AutoMapper;
using System.Text.Json;

namespace AccountManager.Infrastructure.Clients
{
    public partial class RiotClient : IRiotClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AlertService _alertService;
        private readonly IMemoryCache _memoryCache;
        private readonly IDistributedCache _persistantCache;
        private readonly RiotApiUri _riotApiUri;
        private readonly IMapper _autoMapper;
        private readonly ICurlRequestBuilder _curlRequestBuilder;

        public RiotClient(IHttpClientFactory httpClientFactory, AlertService alertService, IMemoryCache memoryCache, 
            IDistributedCache persistantCache, IOptions<RiotApiUri> riotApiOptions, IMapper autoMapper, ICurlRequestBuilder curlRequestBuilder )
        {
            _httpClientFactory = httpClientFactory;
            _alertService = alertService;
            _memoryCache = memoryCache;
            _persistantCache = persistantCache;
            _riotApiUri = riotApiOptions.Value;
            _autoMapper = autoMapper;
            _curlRequestBuilder = curlRequestBuilder;
        }

        public async Task<string?> GetExpectedClientVersion()
        {
            if (_memoryCache.TryGetValue("riot.val.version", out string? version) && version is not null)
                return version;

            var client = _httpClientFactory.CreateClient("Valorant");
            var response = await client.GetFromJsonAsync<ExpectedClientVersionResponse>($"/v1/version");

            _memoryCache.Set("riot.val.version", response?.Data?.RiotClientVersion);
            return response?.Data?.RiotClientVersion;
        }

        private async Task<RiotAuthResponse?> GetRiotSessionCookies(RiotSessionRequest request, Account account)
        {
            var sessionCacheKey = $"{account.Username}.riot.authrequest.{request.GetHashId()}.ssid";
            _memoryCache.TryGetValue(sessionCacheKey, out Cookie? sessionCookie);
            var cookieCollection = new CookieCollection();
            if (sessionCookie is not null)
                cookieCollection.Add(sessionCookie);

            var authResponse = await _curlRequestBuilder.CreateBuilder()
                .SetUri($"{_riotApiUri.Auth}/api/v1/authorization")
                .SetContent(request)
                .AddCookies(cookieCollection)
                .AddHeader("X-Riot-ClientVersion", await GetExpectedClientVersion() ?? "")
                .SetUserAgent("RiotClient/50.0.0.4396195.4381201 rso-auth (Windows;10;;Professional, x64)")
                .Post<TokenResponseWrapper>();

            var authResponseDeserialized = authResponse.ResponseContent;
            RiotAuthResponse authResponseContent = new()
            {
                Content = authResponseDeserialized,
                Cookies = new(authResponse.Cookies ?? new())
            };
                
            if (authResponseContent?.Content?.Type == "response" && authResponseContent?.Cookies?.Ssid is not null)
                _memoryCache.Set(sessionCacheKey, authResponseContent?.Cookies?.Ssid, DateTimeOffset.Now.AddMinutes(55));

            return authResponseContent;
        }

        public async Task<RiotAuthResponse?> RiotAuthenticate(RiotSessionRequest request, Account account)
        {
            RiotAuthCookies responseCookies;
            
            var sessionCacheKey = $"{account.Username}.riot.authrequest.{request.GetHashId()}.ssid";

            if (await _persistantCache.GetAsync<bool>($"{account.Username}.riot.skip.auth"))
                return null;

            var initialAuth = await GetRiotSessionCookies(request, account);
            if (initialAuth?.Content?.Type == "response")
                return initialAuth;

            var initialCookies = initialAuth?.Cookies ?? new();

            var authResponse = await _curlRequestBuilder.CreateBuilder()
            .SetUri($"{_riotApiUri.Auth}/api/v1/authorization")
            .SetContent(new AuthRequest
            {
                Type = "auth",
                Username = account.Username,
                Password = account.Password,
                Remember = true
            })
            .AddHeader("X-Riot-ClientVersion", await GetExpectedClientVersion() ?? "")
            .SetUserAgent("RiotClient/50.0.0.4396195.4381201 rso-auth (Windows;10;;Professional, x64)")
            .AddCookies(initialCookies.GetCookies())
            .Put<TokenResponseWrapper>();

            responseCookies = new(authResponse.Cookies ?? new());

            if (responseCookies?.Ssid is not null)
                _memoryCache.Set(sessionCacheKey, responseCookies?.Ssid, DateTimeOffset.Now.AddMinutes(55));

            var tokenResponse = authResponse.ResponseContent;
            if (tokenResponse?.Type == "multifactor")
            {
                if (string.IsNullOrEmpty(tokenResponse?.Multifactor?.Email))
                {
                    _alertService.AddErrorMessage("Unable to authenticate due to throttling. Try again later.");
                    return null;
                }

                var mfCode = await _alertService.PromptUserFor2FA(account, tokenResponse?.Multifactor?.Email ?? "");
                if (mfCode == string.Empty)
                {
                    await _persistantCache.SetAsync($"{account.Username}.riot.skip.auth", true);
                    return null;
                }

                authResponse = await _curlRequestBuilder.CreateBuilder()
                .SetUri($"{_riotApiUri.Auth}/api/v1/authorization")
                .SetContent(new MultifactorRequest()
                {
                    Code = mfCode,
                    Type = "multifactor",
                    RememberDevice = true
                })
                .AddHeader("X-Riot-ClientVersion", await GetExpectedClientVersion() ?? "")
                .SetUserAgent("RiotClient/50.0.0.4396195.4381201 rso-auth (Windows;10;;Professional, x64)")
                .AddCookies(responseCookies?.GetCookies() ?? new())
                .Put<TokenResponseWrapper>();

                responseCookies = new RiotAuthCookies(authResponse?.Cookies ?? new());
                if (responseCookies?.Ssid is not null)
                    _memoryCache.Set(sessionCacheKey, responseCookies?.Ssid, DateTimeOffset.Now.AddMinutes(55));

                tokenResponse = authResponse?.ResponseContent;

                if (tokenResponse?.Type == "multifactor")
                    _alertService.AddErrorMessage($"Incorrect code. Unable to authenticate {account.Username}");
            }

            var response = new RiotAuthResponse
            {
                Content = tokenResponse,
                Cookies = responseCookies
            };

            return response;
        }

        public async Task<string?> GetValorantToken(Account account)
        {
            var initialAuthTokenRequest = new RiotSessionRequest
            {
                Id = "play-valorant-web-prod",
                Nonce = "1",
                RedirectUri = "https://playvalorant.com/opt_in",
                ResponseType = "token id_token"
            };

            var riotAuthResponse = await RiotAuthenticate(initialAuthTokenRequest, account);

            if (riotAuthResponse is null || riotAuthResponse?.Content?.Response?.Parameters?.Uri is null)
                return null;

            var matches = Regex.Matches(riotAuthResponse.Content.Response.Parameters.Uri,
                    @"access_token=((?:[a-zA-Z]|\d|\.|-|_)*).*id_token=((?:[a-zA-Z]|\d|\.|-|_)*).*expires_in=(\d*)");

            var token = matches[0].Groups[1].Value;

            return token;
        }

        public async Task<string?> GetEntitlementToken(string token)
        {
            var response = await _curlRequestBuilder.CreateBuilder()
            .SetUri($"{_riotApiUri.Entitlement}/api/token/v1")
            .SetContent(new { })
            .SetBearerToken(token)
            .AddHeader("X-Riot-ClientVersion", await GetExpectedClientVersion() ?? "")
            .SetUserAgent("RiotClient/50.0.0.4396195.4381201 rso-auth (Windows;10;;Professional, x64)")
            .AddHeader("X-Riot-ClientPlatform", "ew0KCSJwbGF0Zm9ybVR5cGUiOiAiUEMiLA0KCSJwbGF0Zm9ybU9TIjogIldpbmRvd3MiLA0KCSJwbGF0Zm9ybU9TVmVyc2lvbiI6ICIxMC4wLjE5MDQyLjEuMjU2LjY0Yml0IiwNCgkicGxhdGZvcm1DaGlwc2V0IjogIlVua25vd24iDQp9")
            .Post<EntitlementTokenResponse>();

            return response.ResponseContent?.EntitlementToken;
        }

        public async Task<string?> GetPuuId(string username, string password)
        {
            var bearerToken = await GetValorantToken(new Account
            {
                Username = username,
                Password = password
            });
            if (bearerToken is null)
                return null;

            var entitlementToken = await GetEntitlementToken(bearerToken);
            var cookieCollection = new CookieCollection();

            var response = await _curlRequestBuilder.CreateBuilder()
            .SetUri($"{_riotApiUri.Auth}/userinfo")
            .SetBearerToken(bearerToken)
            .AddCookies(cookieCollection)
            .AddHeader("X-Riot-ClientVersion", await GetExpectedClientVersion() ?? "")
            .SetUserAgent("RiotClient/50.0.0.4396195.4381201 rso-auth (Windows;10;;Professional, x64)")
            .AddHeader("X-Riot-Entitlements-JWT", entitlementToken ?? "")
            .Get<UserInfoResponse>();

            var responseContent = response.ResponseContent;
            var responseCookies = new RiotAuthCookies(response.Cookies ?? new());

            return responseContent?.PuuId;
        }

        public async Task<ValorantRankedHistoryResponse?> GetValorantCompetitiveHistory(Account account, int startIndex, int endIndex)
        {
            var client = _httpClientFactory.CreateClient("ValorantNA");
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-Riot-ClientVersion", await GetExpectedClientVersion());
            var bearerToken = await GetValorantToken(account);
            if (bearerToken is null)
                return new();

            var entitlementToken = await GetEntitlementToken(bearerToken);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-Riot-Entitlements-JWT", entitlementToken);

            var response = await client.GetAsync($"/mmr/v1/players/{account.PlatformId}/competitiveupdates?queue=competitive&startIndex={startIndex}&endIndex={endIndex}");

            return await response.Content.ReadFromJsonAsync<ValorantRankedHistoryResponse>();
        }

        public async Task<IEnumerable<ValorantMatch>?> GetValorantGameHistory(Account account, int startIndex, int endIndex)
        {
            var client = _httpClientFactory.CreateClient("ValorantNA");
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-Riot-ClientVersion", await GetExpectedClientVersion());
            var bearerToken = await GetValorantToken(account);
            if (bearerToken is null)
                return new List<ValorantMatch>();

            var entitlementToken = await GetEntitlementToken(bearerToken);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-Riot-Entitlements-JWT", entitlementToken);

            var gameHistoryDataResponse = await client.GetAsync($"/match-history/v1/history/{account.PlatformId}?queue=competitive&startIndex={startIndex}&endIndex={endIndex}");
            var gameHistoryData = await gameHistoryDataResponse.Content.ReadFromJsonAsync<ValorantGameHistoryDataResponse>();

            var valorantMatches = new List<ValorantMatch>();

            foreach (var game in gameHistoryData?.History ?? new())
            {
                var gameData = await client.GetFromJsonAsync<ValorantMatch>($"/match-details/v1/matches/{game.MatchID}");
                
                if (gameData is not null)
                    valorantMatches.Add(gameData);
            }

            return valorantMatches;
        }

        public async Task<Rank> GetValorantRank(Account account)
        {
            int rankNumber;
            var rankedHistory = await GetValorantCompetitiveHistory(account, 0, 15);

            if (rankedHistory?.Matches?.Any() is false)
                return _autoMapper.Map<ValorantRank>(0);

            var mostRecentMatch = rankedHistory?.Matches?.First();
            rankNumber = mostRecentMatch?.TierAfterUpdate ?? 0;

            var rank = _autoMapper.Map<ValorantRank>(rankNumber);

            return rank;
        }
    }
}
