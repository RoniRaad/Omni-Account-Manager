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
using System.Reflection.Emit;
using System.Web;
using System.Security.Principal;
using System.IdentityModel.Tokens.Jwt;
using System;
using KeyedSemaphores;

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
        private readonly IHttpRequestBuilder _curlRequestBuilder;
        public RiotClient(IHttpClientFactory httpClientFactory, AlertService alertService, IMemoryCache memoryCache, 
            IDistributedCache persistantCache, IOptions<RiotApiUri> riotApiOptions, IMapper autoMapper, IHttpRequestBuilder curlRequestBuilder )
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
            var client = _httpClientFactory.CreateClient("Valorant");
            var response = await client.GetFromJsonAsync<ExpectedClientVersionResponse>($"/v1/version");

            return response?.Data?.RiotClientVersion;
        }

        private async Task<RiotAuthResponse?> CreateRiotSessionCookies(RiotSessionRequest request, Account account)
        {
            var sessionCacheKey = $"{nameof(CreateRiotSessionCookies)}.{account.Username}.riot.authrequest.{request.GetHashId()}.ssid";
            _memoryCache.TryGetValue(sessionCacheKey, out Cookie? sessionCookie);
            var cookieCollection = new CookieCollection();
            if (sessionCookie is not null)
                cookieCollection.Add(sessionCookie);

            var authResponse = await _curlRequestBuilder.CreateBuilder()
                .SetUri($"{_riotApiUri.Auth}/api/v1/authorization")
                .SetContent(request)
                .AddCookies(cookieCollection)
                .AddHeader("X-Riot-ClientVersion", await GetExpectedClientVersion() ?? "")
                .SetUserAgent("RiotClient/51.0.0.4429735.4381201 rso-auth (Windows;10;;Professional, x64)")
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

                        tokenResponse = authResponse?.ResponseContent;

                        if (tokenResponse?.Type == "multifactor")
                            _alertService.AddErrorMessage($"Incorrect code. Unable to authenticate {account.Username}");
                    }

                    if (authResponse?.Cookies is not null)
                        await _persistantCache.SetAsync<RiotAuthCookies>(cookiesCacheKey, responseCookies);

                    response = new RiotAuthResponse
                    {
                        Content = tokenResponse,
                        Cookies = responseCookies
                    };

                    _memoryCache.Set(cacheKey, response, TimeSpan.FromMinutes(55));

                    return response;

                }
                catch
                {
                    return null;
                }
            }
        }

        public async Task<RiotAuthResponse?> RefreshToken(RiotSessionRequest request, RiotAuthCookies cookies)
        {
            var uriParameters = $"redirect_uri={HttpUtility.UrlEncode(request.RedirectUri)}&client_id={HttpUtility.UrlEncode(request.Id)}&response_type={HttpUtility.UrlEncode(request.ResponseType)}&nonce={HttpUtility.UrlEncode(request.Nonce)}&scope={HttpUtility.UrlEncode(request.Scope)}";
            var tokenResponse = await _curlRequestBuilder.CreateBuilder()
                .SetUri($"{_riotApiUri.Auth}/authorize?{uriParameters}")
                .AddHeader("X-Riot-ClientVersion", await GetExpectedClientVersion() ?? "")
                .SetUserAgent("RiotClient/50.0.0.4396195.4381201 rso-auth (Windows;10;;Professional, x64)")
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

            return response?.ResponseContent?.EntitlementToken;
        }

        public async Task<string?> GetPuuId(Account account)
        {
            var idToken = await GetIdToken(account);
            if (string.IsNullOrEmpty(idToken))
                return "";

            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(idToken);
            jwtSecurityToken.Payload.TryGetValue("sub", out var puuid);

            return puuid?.ToString() ?? "";
        }

        public async Task<string?> GetIdToken(Account account)
        {
            var request = new RiotSessionRequest
            {
                Id = "lol",
                Nonce = "1",
                RedirectUri = "http://localhost/redirect",
                ResponseType = "token id_token",
                Scope = "openid link ban lol_region"
            };

            var response = await RiotAuthenticate(request, account);

            var responseContent = response?.Content?.Response?.Parameters?.Uri;
            if (responseContent is null)
                return "";

            var redirectUri = new Uri(responseContent);
            var queryParsed = HttpUtility.ParseQueryString(redirectUri.Fragment);
            var idToken = queryParsed.Get("id_token") ?? "";

            return idToken;
        }
    }
}
