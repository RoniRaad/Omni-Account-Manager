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

        public RiotClient(IHttpClientFactory httpClientFactory, AlertService alertService, IMemoryCache memoryCache, 
            IDistributedCache persistantCache, IOptions<RiotApiUri> riotApiOptions, IMapper autoMapper)
        {
            _httpClientFactory = httpClientFactory;
            _alertService = alertService;
            _memoryCache = memoryCache;
            _persistantCache = persistantCache;
            _riotApiUri = riotApiOptions.Value;
            _autoMapper = autoMapper;
        }

        private async Task AddHeadersToClient(HttpClient httpClient)
        {
            if (httpClient.DefaultRequestHeaders.Contains("X-Riot-ClientVersion"))
                return;

            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Riot-ClientVersion", await GetExpectedClientVersion());
        }

        public async Task<string?> GetExpectedClientVersion()
        {
            var client = _httpClientFactory.CreateClient("Valorant");
            var response = await client.GetFromJsonAsync<ExpectedClientVersionResponse>($"/v1/version");
            return response?.Data?.RiotClientVersion;
        }

        private async Task<RiotAuthResponse?> GetRiotSessionCookies(RiotSessionRequest request, Account account)
        {
            var tdidCacheKey = $"riot.auth.tdid";
            var sessionCacheKey = $"{account.Username}.riot.authrequest.{request.GetHashId()}.ssid";
            _memoryCache.TryGetValue(sessionCacheKey, out var sessionCookie);
            var cachedTdidCookie = await _persistantCache.GetStringAsync(tdidCacheKey);
            var cookieHeader = "";
            var client = _httpClientFactory.CreateClient("RiotAuth");
            await AddHeadersToClient(client);
            if (cachedTdidCookie is not null)
                cookieHeader += cachedTdidCookie + ";";

            if (sessionCookie is not null)
                cookieHeader += sessionCookie + ";";

            client.DefaultRequestHeaders.Add("Cookie", cookieHeader.Split(";"));

            var authResponse = await client.PostAsJsonAsync($"/api/v1/authorization", request);
            authResponse.EnsureSuccessStatusCode();

            var authResponseDeserialized = await authResponse.Content.ReadFromJsonAsync<TokenResponseWrapper>();
            RiotAuthResponse authObject = new()
            {
                Content = authResponseDeserialized,
                Cookies = new(authResponse.Headers.ToList())
            };
                
            if (authObject?.Content?.Type == "response" && authObject?.Cookies?.Ssid is not null)
                _memoryCache.Set(sessionCacheKey, authObject?.Cookies?.Ssid, DateTimeOffset.Now.AddMinutes(55));

            if (authObject?.Cookies?.Tdid is not null)
                await _persistantCache.SetStringAsync(tdidCacheKey, authObject.Cookies.Tdid);

            return authObject;
        }

        public async Task<RiotAuthResponse?> RiotAuthenticate(RiotSessionRequest request, Account account)
        {
            RiotAuthCookies responseCookies;
            var tdidCacheKey = $"riot.auth.tdid";
            var sessionCacheKey = $"{account.Username}.riot.authrequest.{request.GetHashId()}.ssid";

            if (await _persistantCache.GetAsync<bool>($"{account.Username}.riot.skip.auth"))
                return null;

            var initialAuth = await GetRiotSessionCookies(request, account);
            if (initialAuth?.Content?.Type == "response")
                return initialAuth;

            var initialCookies = initialAuth?.Cookies ?? new();
            var cookieHeader = initialCookies.GetCookieHeader();
            var client = _httpClientFactory.CreateClient("RiotAuth");

            await AddHeadersToClient(client);
            client.DefaultRequestHeaders.Add("Cookie", cookieHeader.Split(";"));

            var authRequest = new HttpRequestMessage(HttpMethod.Put, "/api/v1/authorization");
            authRequest.Version = HttpVersion.Version20;
            authRequest.Content = JsonContent.Create(new AuthRequest
            {
                Type = "auth",
                Username = account.Username,
                Password = account.Password,
                Remember = true
            });

            authRequest.Headers.Add("Cookie", cookieHeader.Split(";"));
            authRequest.Version = HttpVersion.Version20;

            var authResponse = await client.SendAsync(authRequest);
            authResponse.EnsureSuccessStatusCode();

            responseCookies = new(authResponse.Headers.ToList());
            if (responseCookies?.Ssid is not null)
                _memoryCache.Set(sessionCacheKey, responseCookies?.Ssid, DateTimeOffset.Now.AddMinutes(55));

            if (responseCookies?.Tdid is not null)
                await _persistantCache.SetStringAsync(tdidCacheKey, responseCookies?.Tdid);

            var tokenResponse = await authResponse.Content.ReadFromJsonAsync<TokenResponseWrapper>();
            if (tokenResponse?.Type == "multifactor")
            {
                if (string.IsNullOrEmpty(tokenResponse?.Multifactor?.Email))
                {
                    _alertService.AddErrorMessage("Unable to authenticate due to throttling. Try again later.");
                    return null;
                }

                if (responseCookies?.Tdid is not null)
                    _alertService.AddErrorMessage($"MFA with cached cookie! Value {responseCookies?.Tdid}");
                else
                    _alertService.AddInfoMessage("MFA without cached cookie!");

                var mfCode = await _alertService.PromptUserFor2FA(account, tokenResponse?.Multifactor?.Email ?? "");
                if (mfCode == string.Empty)
                {
                    await _persistantCache.SetAsync($"{account.Username}.riot.skip.auth", true);
                    return null;
                }

                var mfaRequest = new HttpRequestMessage(HttpMethod.Put, "/api/v1/authorization");
                mfaRequest.Version = HttpVersion.Version20;
                mfaRequest.Content = JsonContent.Create(new MultifactorRequest()
                {
                    Code = mfCode,
                    Type = "multifactor",
                    RememberDevice = true
                });
                mfaRequest.Version = HttpVersion.Version20;
                mfaRequest.Headers.Add("Cookie", responseCookies?.GetCookieHeader() ?? "");
                authResponse = await client.SendAsync(mfaRequest);
                authResponse.EnsureSuccessStatusCode();

                responseCookies = new RiotAuthCookies(authResponse.Headers.ToList());
                if (responseCookies?.Ssid is not null)
                    _memoryCache.Set(sessionCacheKey, responseCookies?.Ssid, DateTimeOffset.Now.AddMinutes(55));

                if (responseCookies?.Tdid is not null)
                    await _persistantCache.SetStringAsync(tdidCacheKey, responseCookies?.Tdid);

                tokenResponse = await authResponse.Content.ReadFromJsonAsync<TokenResponseWrapper>();

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
            var client = _httpClientFactory.CreateClient("Entitlement");

            await AddHeadersToClient(client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var entitlementResponse = await client.PostAsJsonAsync($"/api/token/v1", new { });
            var entitlementResponseDeserialized = await entitlementResponse.Content.ReadFromJsonAsync<EntitlementTokenResponse>();

            return entitlementResponseDeserialized?.EntitlementToken;
        }

        public async Task<string?> GetPuuId(string username, string password)
        {
            var tdidCacheKey = $"riot.auth.tdid";
            var client = _httpClientFactory.CreateClient("RiotAuth");
            await AddHeadersToClient(client);

            var bearerToken = await GetValorantToken(new Account
            {
                Username = username,
                Password = password
            });
            if (bearerToken is null)
                return null;

            var tdidCookie = _persistantCache.GetString(tdidCacheKey);
            var entitlementToken = await GetEntitlementToken(bearerToken);

            var request = new HttpRequestMessage(HttpMethod.Get, "/userinfo");
            request.Version = HttpVersion.Version20;
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            request.Headers.Add("Cookie", tdidCookie);
            request.Headers.TryAddWithoutValidation("X-Riot-Entitlements-JWT", entitlementToken);

            var response = await client.SendAsync(request);
            var responseContent = await response.Content.ReadFromJsonAsync<UserInfoResponse>();
            var responseCookies = new RiotAuthCookies(response.Headers.ToList());

            if (!string.IsNullOrEmpty(responseCookies.Tdid))
                _persistantCache.SetString(tdidCacheKey, responseCookies.Tdid);

            return responseContent?.PuuId;
        }

        public async Task<ValorantRankedResponse?> GetValorantCompetitiveHistory(Account account)
        {
            var client = _httpClientFactory.CreateClient("ValorantNA");
            await AddHeadersToClient(client);
            var bearerToken = await GetValorantToken(account);
            if (bearerToken is null)
                return new();

            var entitlementToken = await GetEntitlementToken(bearerToken);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-Riot-Entitlements-JWT", entitlementToken);

            var response = await client.GetAsync($"/mmr/v1/players/{account.PlatformId}/competitiveupdates?queue=competitive&startIndex=0&endIndex=15");

            return await response.Content.ReadFromJsonAsync<ValorantRankedResponse>();
        }

        public async Task<Rank> GetValorantRank(Account account)
        {
            int rankNumber;
            var rankedHistory = await GetValorantCompetitiveHistory(account);

            if (rankedHistory?.Matches?.Any() is false)
                return _autoMapper.Map<ValorantRank>(0);

            var mostRecentMatch = rankedHistory?.Matches?.First();
            rankNumber = mostRecentMatch?.TierAfterUpdate ?? 0;

            var rank = _autoMapper.Map<ValorantRank>(rankNumber);

            return rank;
        }
    }
}
