using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Models.RiotGames;
using AccountManager.Core.Models.RiotGames.Valorant;
using AccountManager.Core.Models.RiotGames.Valorant.Responses;
using Microsoft.Extensions.Caching.Distributed;
using System.Net;
using AccountManager.Core.Static;
using AccountManager.Core.Models.RiotGames.Requests;
using AccountManager.Core.Models.AppSettings;
using Microsoft.Extensions.Options;
using AutoMapper;
using System.Web;
using KeyedSemaphores;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using LazyCache;

namespace AccountManager.Infrastructure.Clients
{
    public sealed class RiotTokenClient : IRiotTokenClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAlertService _alertService;
        private readonly ILogger<RiotTokenClient> _logger;
        private readonly IAppCache _memoryCache;
        private readonly IDistributedCache _persistantCache;
        private readonly RiotApiUri _riotApiUri;
        private readonly IRiotThirdPartyClient _riot3rdPartyClient;
        private readonly IMapper _autoMapper;
        private readonly IHttpRequestBuilder _curlRequestBuilder;
        public static readonly ImmutableDictionary<string, string> RiotAuthRegionMapping = new Dictionary<string, string>()
                        {
                            {"na", "usw" },
                            {"latam", "usw" },
                            {"br", "usw" },
                            {"eu", "euc" },
                            {"ap", "apse" },
                            {"kr", "apse" }
                        }.ToImmutableDictionary();
        public RiotTokenClient(IHttpClientFactory httpClientFactory, IAlertService alertService, IAppCache memoryCache,
            IDistributedCache persistantCache, IOptions<RiotApiUri> riotApiOptions, IMapper autoMapper, IHttpRequestBuilder curlRequestBuilder, 
            ILogger<RiotTokenClient> logger, IRiotThirdPartyClient riot3rdPartyClient)
        {
            _httpClientFactory = httpClientFactory;
            _alertService = alertService;
            _memoryCache = memoryCache;
            _persistantCache = persistantCache;
            _riotApiUri = riotApiOptions.Value;
            _autoMapper = autoMapper;
            _curlRequestBuilder = curlRequestBuilder;
            _logger = logger;
            _riot3rdPartyClient = riot3rdPartyClient;
        }

        public async Task<string?> GetExpectedClientVersion()
        {
            var versionInfo = await _riot3rdPartyClient.GetRiotVersionInfoAsync();
            return versionInfo?.Data?.RiotClientVersion;
        }

        private async Task<RiotAuthResponse?> CreateRiotSessionCookies(RiotTokenRequest request, Account account)
        {
            var sessionCacheKey = $"{nameof(CreateRiotSessionCookies)}.{account.Username}.riot.authrequest.{request.GetHashId()}.ssid";
            _memoryCache.TryGetValue(sessionCacheKey, out Cookie? sessionCookie);
            var cookieCollection = new CookieCollection();
            if (sessionCookie is not null)
                cookieCollection.Add(sessionCookie);

            var authResponse = await _curlRequestBuilder.CreateBuilder()
                .SetUri($"{_riotApiUri.Auth}/api/v1/authorization/")
                .SetContent(request)
                .AddCookies(cookieCollection)
                .AddHeader("X-Riot-ClientVersion", await GetExpectedClientVersion() ?? "")
                .SetUserAgent("Rito") // This is a bypass for riot blocking our useragent.
                .Post<TokenResponseWrapper>();

            var authResponseDeserialized = authResponse.ResponseContent;
            RiotAuthResponse authResponseContent = new()
            {
                Content = authResponseDeserialized,
                Cookies = new(authResponse.Cookies ?? new())
            };

            if (authResponseContent?.Content?.Type == "response" && authResponseContent?.Cookies?.Ssid is not null)
                _memoryCache.Add(sessionCacheKey, authResponseContent?.Cookies?.Ssid, DateTimeOffset.Now.AddMinutes(55));

            return authResponseContent;
        }

        private async Task<RiotAuthResponse?> RiotAuthenticate(RiotTokenRequest request, Account account)
        {
            var cacheKey = $"{account.Username}.{request.GetHashId()}.{nameof(RiotAuthenticate)}";
            RiotAuthCookies responseCookies;
            RiotAuthResponse? response;

            using (await KeyedSemaphore.LockAsync(account.Username))
            {
                try
                {
                    var cookiesCacheKey = $"{nameof(RiotAuthenticate)}.{account.Username}.riot.authrequest.{request.GetHashId()}.cookies";
                    var cookies = await _persistantCache.GetAsync<RiotAuthCookies>(cookiesCacheKey);

                    if (cookies is not null)
                    {
                        var token = await RefreshToken(request, cookies);
                        if (token?.Content?.Type == "response")
                            return token;
                    }

                    if (await _persistantCache.GetAsync<bool>($"{account.Username}.riot.skip.auth"))
                        return null;

                    var initialAuth = await CreateRiotSessionCookies(request, account);
                    var initialCookies = initialAuth?.Cookies ?? new();

                    if (initialAuth?.Content?.Type == "response")
                    {
                        if (initialAuth?.Cookies is not null)
                            await _persistantCache.SetAsync(cookiesCacheKey, initialCookies);

                        return initialAuth;
                    }

                    var authResponse = await _curlRequestBuilder.CreateBuilder()
                    .SetUri($"{_riotApiUri.Auth}/api/v1/authorization/")
                    .SetContent(new AuthRequest
                    {
                        Type = "auth",
                        Username = account.Username,
                        Password = account.Password,
                        Remember = true
                    })
                    .AddHeader("X-Riot-ClientVersion", await GetExpectedClientVersion() ?? "")
                    .SetUserAgent(await GetRiotClientUserAgent())
                    .AddCookies(initialCookies.GetCookies())
                    .Put<TokenResponseWrapper>();

                    responseCookies = new(authResponse.Cookies ?? new());

                    var tokenResponse = authResponse.ResponseContent;
                    if (tokenResponse?.Type == "multifactor")
                    {
                        if (string.IsNullOrEmpty(tokenResponse?.Multifactor?.Email))
                        {
                            _alertService.AddErrorAlert("Unable to authenticate due to throttling. Try again later.");
                            return null;
                        }

                        var mfCode = await _alertService.PromptUserFor2FA(account, tokenResponse?.Multifactor?.Email ?? "");
                        if (mfCode == string.Empty)
                        {
                            await _persistantCache.SetAsync($"{account.Username}.riot.skip.auth", true);
                            return null;
                        }

                        authResponse = await _curlRequestBuilder.CreateBuilder()
                        .SetUri($"{_riotApiUri.Auth}/api/v1/authorization/")
                        .SetContent(new MultifactorRequest()
                        {
                            Code = mfCode,
                            Type = "multifactor",
                            RememberDevice = true
                        })
                        .AddHeader("X-Riot-ClientVersion", await GetExpectedClientVersion() ?? "")
                        .SetUserAgent(await GetRiotClientUserAgent())
                        .AddCookies(responseCookies?.GetCookies() ?? new())
                        .Put<TokenResponseWrapper>();


                        responseCookies = new RiotAuthCookies(authResponse?.Cookies ?? new());

                        tokenResponse = authResponse?.ResponseContent;

                        if (tokenResponse?.Type == "multifactor")
                            _alertService.AddErrorAlert($"Incorrect code. Unable to authenticate {account.Username}");
                    }

                    if (authResponse?.Cookies is not null)
                        await _persistantCache.SetAsync<RiotAuthCookies>(cookiesCacheKey, responseCookies);

                    response = new RiotAuthResponse
                    {
                        Content = tokenResponse,
                        Cookies = responseCookies
                    };

                    _memoryCache.Add(cacheKey, response, TimeSpan.FromMinutes(55));

                    return response;

                }
                catch
                {
                    return null;
                }
            }
        }

        public async Task<RiotAuthResponse?> RefreshToken(RiotTokenRequest request, RiotAuthCookies cookies)
        {
            var uriParameters = $"redirect_uri={HttpUtility.UrlEncode(request.RedirectUri)}&client_id={HttpUtility.UrlEncode(request.Id)}&response_type={HttpUtility.UrlEncode(request.ResponseType)}&nonce={HttpUtility.UrlEncode(request.Nonce)}&scope={HttpUtility.UrlEncode(request.Scope)}";
            var tokenResponse = await _curlRequestBuilder.CreateBuilder()
                .SetUri($"{_riotApiUri.Auth}/authorize?{uriParameters}/")
                .AddHeader("X-Riot-ClientVersion", await GetExpectedClientVersion() ?? "")
                .SetUserAgent(await GetRiotClientUserAgent())
                .AddCookies(cookies.GetCookies() ?? new())
                .Get();

            var responseCookies = new RiotAuthCookies(tokenResponse?.Cookies ?? new());
            var location = tokenResponse?.Location;
            var type = tokenResponse?.StatusCode == HttpStatusCode.RedirectMethod ? "response" : "none";

            var response = new RiotAuthResponse
            {
                Content = new TokenResponseWrapper
                {
                    Response = new()
                    {
                        Parameters = new()
                        {
                            Uri = tokenResponse?.Location
                        }
                    },
                    Type = type
                },
                Cookies = responseCookies
            };

            if (location is null)
                return response;

            if (location?.StartsWith("/login") is true || location?.StartsWith("\\login") is true)
            {
                response.Content.Type = "invalid_session";
                return response;
            }

            var locationUri = new Uri(location ?? "");
            var fragment = locationUri.Fragment[1..];
            var parsedFragment = HttpUtility.ParseQueryString(fragment);

            if (type != "none")
            {

                if (!parsedFragment.AllKeys.Contains("access_token") || location?.Contains("error") is true)
                {
                    response.Content.Type = "error";
                    return response;
                }

                if (location is null)
                {
                    response.Content.Type = "none";
                    return response;
                }
            }

            return response;
        }

        public async Task<string?> GetEntitlementToken(string accessToken)
        {
            var response = await _curlRequestBuilder.CreateBuilder()
            .SetUri($"{_riotApiUri.Entitlement}/api/token/v1/")
            .SetContent(new { })
            .SetBearerToken(accessToken)
            .AddHeader("X-Riot-ClientVersion", await GetExpectedClientVersion() ?? "")
            .SetUserAgent(await GetRiotClientUserAgent())
            .AddHeader("X-Riot-ClientPlatform", "ew0KCSJwbGF0Zm9ybVR5cGUiOiAiUEMiLA0KCSJwbGF0Zm9ybU9TIjogIldpbmRvd3MiLA0KCSJwbGF0Zm9ybU9TVmVyc2lvbiI6ICIxMC4wLjE5MDQyLjEuMjU2LjY0Yml0IiwNCgkicGxhdGZvcm1DaGlwc2V0IjogIlVua25vd24iDQp9")
            .Post<EntitlementTokenResponse>();

            return response?.ResponseContent?.EntitlementToken;
        }

        public async Task<RiotAuthTokensResponse> GetRiotTokens(RiotTokenRequest request, Account account)
        {
            var response = await RiotAuthenticate(request, account);

            var responseContent = response?.Content?.Response?.Parameters?.Uri;
            if (responseContent is null)
                return new();

            var redirectUri = new Uri(responseContent);
            var queryParsed = HttpUtility.ParseQueryString(redirectUri.Fragment);
            var accessToken = queryParsed.Get("access_token") ?? queryParsed.Get("#access_token") ?? "";
            var idToken = queryParsed.Get("id_token") ?? "";

            if (!int.TryParse(queryParsed.Get("expires_in"), out int expiresIn))
                expiresIn = 0;

            return new() { AccessToken = accessToken, IdToken = idToken, ExpiresIn = expiresIn, Cookies = response?.Cookies ?? new()};
        }

        private async Task<string> GetRiotClientUserAgent()
        {
            var versionInfo = await _riot3rdPartyClient.GetRiotVersionInfoAsync();

            return _riotApiUri.UserAgentTemplate.Replace("{riotClientBuild}", versionInfo?.Data?.RiotClientBuild ?? "");
        }
    }
}
