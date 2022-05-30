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

            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Riot-ClientPlatform", "ew0KCSJwbGF0Zm9ybVR5cGUiOiAiUEMiLA0KCSJwbGF0Zm9ybU9TIjogIldpbmRvd3MiLA0KCSJwbGF0Zm9ybU9TVmVyc2lvbiI6ICIxMC4wLjE5MDQyLjEuMjU2LjY0Yml0IiwNCgkicGxhdGZvcm1DaGlwc2V0IjogIlVua25vd24iDQp9");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Riot-ClientVersion", await GetExpectedClientVersion());
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("RiotClient/50.0.0.4396195.4381201 rso-auth (Windows;10;;Professional, x64)");
            httpClient.DefaultRequestVersion = HttpVersion.Version20;
        }

        public async Task<string?> GetExpectedClientVersion()
        {
            var client = _httpClientFactory.CreateClient("CloudflareBypass");
            var response = await client.GetFromJsonAsync<ExpectedClientVersionResponse>($"{_riotApiUri.Valorant}/v1/version");
            return response?.Data?.RiotClientVersion;
        }

        private async Task<RiotAuthResponse?> GetRiotSessionCookies(RiotSessionRequest request, Account account)
        {
            var tdidCacheKey = $"{account.Username}.riot.auth.tdid";
            var sessionCacheKey = $"{account.Username}.riot.authrequest.{request.GetHashId()}.ssid";
            var cachedSessionCookie = _memoryCache.Get<Cookie>(sessionCacheKey);
            var cachedTdidCookie = await _persistantCache.GetAsync<Cookie>(tdidCacheKey);
            var cookieContainer = new CookieContainer();
            if (cachedSessionCookie is not null && !cachedSessionCookie.Expired)
                cookieContainer.Add(cachedSessionCookie);
            if (cachedTdidCookie is not null && !cachedTdidCookie.Expired)
                cookieContainer.Add(cachedTdidCookie);

            var innerHandler = new HttpClientHandler()
            {
                CookieContainer = cookieContainer
            };
            var handler = new ClearanceHandler(innerHandler)
            {
                MaxRetries = 2
            };

            using (var client = new HttpClient(handler))
            {
                await AddHeadersToClient(client);
                HttpResponseMessage authResponse;
                authResponse = await client.PostAsJsonAsync($"{_riotApiUri.Auth}/api/v1/authorization", request);

                var authResponseDeserialized = await authResponse.Content.ReadFromJsonAsync<TokenResponseWrapper>();
                RiotAuthResponse authObject = new ()
                {
                    Content = authResponseDeserialized,
                    Cookies = new(cookieContainer.GetAllCookies())
                };
                
                if (authObject?.Content?.Type == "response" && authObject?.Cookies?.Ssid is not null && !authObject?.Cookies?.Ssid?.Expired is false)
                    _memoryCache.Set(sessionCacheKey, authObject?.Cookies?.Ssid, authObject.Cookies.Ssid.Expires.AddMinutes(-5));

                if (authObject?.Cookies?.Tdid is not null && authObject?.Cookies?.Tdid?.Expired is false)
                    await _persistantCache.SetAsync(tdidCacheKey, authObject.Cookies.Tdid);

                return authObject;
            }
        }

        public async Task<RiotAuthResponse?> RiotAuthenticate(RiotSessionRequest request, Account account)
        {
            if (await _persistantCache.GetAsync<bool>($"{account.Username}.riot.skip.auth"))
                return null;

            var initialAuth = await GetRiotSessionCookies(request, account);
            initialAuth?.Cookies?.ClearExpiredCookies();
            if (initialAuth?.Content?.Type == "response" && initialAuth?.Cookies?.Validate() is true)
                return initialAuth;

            var initialCookies = initialAuth?.Cookies ?? new();

            var tdidCacheKey = $"{account.Username}.riot.auth.tdid";
            var sessionCacheKey = $"{account.Username}.riot.authrequest.{request.GetHashId()}.ssid";

            var cookieContainer = new CookieContainer();
            cookieContainer.Add(initialCookies.GetCollection());

            var cachedSessionCookie = await _persistantCache.GetAsync<Cookie>(tdidCacheKey);
            if (cachedSessionCookie is not null)
                cookieContainer.Add(cachedSessionCookie);

            var innerHandler = new HttpClientHandler()
            {
                CookieContainer = cookieContainer
            };

            var handler = new ClearanceHandler(innerHandler)
            {
                MaxRetries = 2
            };

            using (var client = new HttpClient(handler))
            {
                await AddHeadersToClient(client);
                //var response = new CurlResponse<TokenResponseWrapper>();
                
                /*await CliWrap.Cli.Wrap("curl")
                .WithArguments((builder) => builder
                .Add("-i -X PUT", false)
                .Add("-H").Add("Content-Type: application/json")
                .Add("-H").Add($"User-Agent: RiotClient/50.0.0.4396195.4381201 rso-auth (Windows;10;;Professional, x64)")
                .Add("-d").Add(JsonSerializer.Serialize(new AuthRequest
                {
                    Type = "auth",
                    Username = account.Username,
                    Password = account.Password,
                    Remember = true
                }))
                .Add($"{_riotApiUri.Auth}/api/v1/authorization"))
                .WithValidation(CliWrap.CommandResultValidation.None)
                .WithStandardOutputPipe(response.GetPipeTarget())
                .ExecuteAsync();
                */

                HttpResponseMessage authResponse = await client.PutAsJsonAsync($"{_riotApiUri.Auth}/api/v1/authorization", new AuthRequest
                {
                    Type = "auth",
                    Username = account.Username,
                    Password = account.Password,
                    Remember = true
                });

                var tokenResponse = await authResponse.Content.ReadFromJsonAsync<TokenResponseWrapper>();

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

                    authResponse = await client.PutAsJsonAsync($"{_riotApiUri.Auth}/api/v1/authorization", new MultifactorRequest()
                    {
                        Code = mfCode,
                        Type = "multifactor",
                        RememberDevice = true
                    });

                    tokenResponse = await authResponse.Content.ReadFromJsonAsync<TokenResponseWrapper>();

                    if (tokenResponse?.Type == "multifactor")
                        _alertService.AddErrorMessage($"Incorrect code. Unable to authenticate {account.Username}");
                }

                var cookies = cookieContainer.GetAllCookies();
                var tdidCookie = cookies.FirstOrDefault((cookie) => cookie?.Name?.ToLower() == "tdid", null);
                var ssidCookie = cookies.FirstOrDefault((cookie) => cookie?.Name?.ToLower() == "ssid", null);

                if (tdidCookie is not null)
                    await _persistantCache.SetAsync(tdidCacheKey, tdidCookie);
                if (ssidCookie is not null)
                    _memoryCache.Set(sessionCacheKey, ssidCookie, ssidCookie.Expires.AddMinutes(-5));

                var response = new RiotAuthResponse
                {
                    Content = tokenResponse,
                    Cookies = new(cookies)
                };

                return response;
            }
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
            var client = _httpClientFactory.CreateClient("CloudflareBypass");

            await AddHeadersToClient(client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var entitlementResponse = await client.PostAsJsonAsync($"{_riotApiUri.Entitlement}/api/token/v1", new { });
            var entitlementResponseDeserialized = await entitlementResponse.Content.ReadFromJsonAsync<EntitlementTokenResponse>();

            return entitlementResponseDeserialized?.EntitlementToken;
        }

        public async Task<string?> GetPuuId(string username, string password)
        {
            var client = _httpClientFactory.CreateClient("CloudflareBypass");
            await AddHeadersToClient(client);

            var bearerToken = await GetValorantToken(new Account
            {
                Username = username,
                Password = password
            });
            if (bearerToken is null)
                return null;

            var entitlementToken = await GetEntitlementToken(bearerToken);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-Riot-Entitlements-JWT", entitlementToken);

            var response = await client.GetFromJsonAsync<UserInfoResponse>($"{_riotApiUri.Auth}/userinfo");
            return response?.PuuId;
        }

        public async Task<ValorantRankedResponse?> GetValorantCompetitiveHistory(Account account)
        {
            var client = _httpClientFactory.CreateClient("CloudflareBypass");
            await AddHeadersToClient(client);
            var bearerToken = await GetValorantToken(account);
            if (bearerToken is null)
                return new();

            var entitlementToken = await GetEntitlementToken(bearerToken);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-Riot-Entitlements-JWT", entitlementToken);

            var response = await client.GetAsync($"{_riotApiUri.ValorantNA}/mmr/v1/players/{account.PlatformId}/competitiveupdates?queue=competitive&startIndex=0&endIndex=15");

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
