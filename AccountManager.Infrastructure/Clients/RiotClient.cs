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
        private static readonly SemaphoreSlim _semaphore = new(1);
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
            if (_memoryCache.TryGetValue(cacheKey, out RiotAuthResponse? response)) 
            {
                return response;
            }
            try
            {
                await _semaphore.WaitAsync();
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
            finally
            {
                _semaphore.Release(1);
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

        public async Task<string?> GetValorantToken(Account account)
        {
            var cacheKey = $"{account.Username}.{nameof(GetValorantToken)}";
            if (_memoryCache.TryGetValue(cacheKey, out string? valorantToken))
                return valorantToken;

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

            if (!string.IsNullOrEmpty(token))
                _memoryCache.Set(cacheKey, token, DateTime.Now.AddMinutes(55));

            return token;
        }

        public async Task<string?> GetEntitlementToken(string token)
        {
            var cacheKey = $"{token}.{nameof(GetEntitlementToken)}";
            _memoryCache.TryGetValue(cacheKey, out string? entitlement);

            if (!string.IsNullOrEmpty(entitlement))
                return entitlement;

            var response = await _curlRequestBuilder.CreateBuilder()
            .SetUri($"{_riotApiUri.Entitlement}/api/token/v1")
            .SetContent(new { })
            .SetBearerToken(token)
            .AddHeader("X-Riot-ClientVersion", await GetExpectedClientVersion() ?? "")
            .SetUserAgent("RiotClient/50.0.0.4396195.4381201 rso-auth (Windows;10;;Professional, x64)")
            .AddHeader("X-Riot-ClientPlatform", "ew0KCSJwbGF0Zm9ybVR5cGUiOiAiUEMiLA0KCSJwbGF0Zm9ybU9TIjogIldpbmRvd3MiLA0KCSJwbGF0Zm9ybU9TVmVyc2lvbiI6ICIxMC4wLjE5MDQyLjEuMjU2LjY0Yml0IiwNCgkicGxhdGZvcm1DaGlwc2V0IjogIlVua25vd24iDQp9")
            .Post<EntitlementTokenResponse>();

            if (!string.IsNullOrEmpty(response?.ResponseContent?.EntitlementToken))
                _memoryCache.Set(cacheKey, response.ResponseContent.EntitlementToken, DateTime.Now.AddMinutes(55));

            return response?.ResponseContent?.EntitlementToken;
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

            return responseContent?.PuuId;
        }

        public async Task<ValorantRankedHistoryResponse?> GetValorantCompetitiveHistory(Account account, int startIndex, int endIndex)
        {
            var cacheKey = $"{account.Username}.{startIndex}.{endIndex}.{nameof(GetValorantCompetitiveHistory)}";
            if (_memoryCache.TryGetValue(cacheKey, out ValorantRankedHistoryResponse? rankedHistory))
                return rankedHistory;

            var client = _httpClientFactory.CreateClient("ValorantNA");
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-Riot-ClientVersion", await GetExpectedClientVersion());
            var bearerToken = await GetValorantToken(account);
            if (bearerToken is null)
                return new();

            var entitlementToken = await GetEntitlementToken(bearerToken);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-Riot-Entitlements-JWT", entitlementToken);

            var response = await client.GetAsync($"/mmr/v1/players/{account.PlatformId}/competitiveupdates?queue=competitive&startIndex={startIndex}&endIndex={endIndex}");
            rankedHistory = await response.Content.ReadFromJsonAsync<ValorantRankedHistoryResponse>();
            
            if (rankedHistory is not null)
                _memoryCache.Set(cacheKey, rankedHistory);

            return rankedHistory;
        }

        private async Task<ValorantStoreTotalOffers?> GetAllShopOffers(Account account)
        {
            var cacheKey = $"{account.Username}.{nameof(GetAllShopOffers)}";
            var offers = await _persistantCache.GetAsync<ValorantStoreTotalOffers>(cacheKey);

            if (offers is not null)
                return offers;

            var client = _httpClientFactory.CreateClient("ValorantNA");
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-Riot-ClientVersion", await GetExpectedClientVersion());
            var bearerToken = await GetValorantToken(account);
            if (bearerToken is null)
                return new();

            var entitlementToken = await GetEntitlementToken(bearerToken);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-Riot-Entitlements-JWT", entitlementToken);

            var response = await client.GetAsync($"/store/v1/offers/");
            offers = await response.Content.ReadFromJsonAsync<ValorantStoreTotalOffers>();

            if (offers is not null)
                await _persistantCache.SetAsync(cacheKey, offers);

            return offers;
        }

        private async Task<ValorantSkinLevelResponse> GetSkinFromUuid(string uuid)
        {
            var client = _httpClientFactory.CreateClient();
            return await client.GetFromJsonAsync<ValorantSkinLevelResponse>($"https://valorant-api.com/v1/weapons/skinlevels/{uuid}") ?? new();
        }

        public async Task<List<ValorantSkinLevelResponse>> GetValorantShopDeals(Account account)
        {
            var cacheKey = $"{account.Username}.{nameof(GetValorantShopDeals)}";
            var offers = await _persistantCache.GetAsync<List<ValorantSkinLevelResponse>>(cacheKey);
            if (offers is not null)
                return offers;

            offers = new();

            var valClient = _httpClientFactory.CreateClient("ValorantNA");
            valClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Riot-ClientVersion", await GetExpectedClientVersion());
            var bearerToken = await GetValorantToken(account);
            if (bearerToken is null)
                return new();

            var entitlementToken = await GetEntitlementToken(bearerToken);

            valClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            valClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Riot-Entitlements-JWT", entitlementToken);

            var responseObj = await valClient.GetAsync($"/store/v2/storefront/{account.PlatformId}");
            var response = await responseObj.Content.ReadFromJsonAsync<ValorantShopOffers>();
            var allOffers = await GetAllShopOffers(account);
            foreach (var offer in response?.SkinsPanelLayout?.SingleItemOffers ?? new())
            {
                var skin = await GetSkinFromUuid(offer);
                offers.Add(skin);
                skin.Data.Price = allOffers?.Offers?.FirstOrDefault(allOffer => allOffer?.OfferID == offer)?.Cost._85ad13f73d1b51289eb27cd8ee0b5741 ?? 0;
            }

            var referenceTimeZone = TimeZoneInfo.FindSystemTimeZoneById("US Eastern Standard Time");
            var currentDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, referenceTimeZone);
            var wantedDateTime = new DateTime(currentDateTime.Year, currentDateTime.Month, currentDateTime.Day);
            await _persistantCache.SetAsync(cacheKey, offers, wantedDateTime.AddHours(20));

            return offers;
        }

        public async Task<IEnumerable<ValorantMatch>?> GetValorantGameHistory(Account account, int startIndex, int endIndex)
        {
            var cacheKey = $"{account.Username}.{startIndex}.{endIndex}.{nameof(GetValorantGameHistory)}";
            if (_memoryCache.TryGetValue(cacheKey, out List<ValorantMatch>? valorantMatches))
                return valorantMatches;
            
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

            valorantMatches = new List<ValorantMatch>();

            foreach (var game in gameHistoryData?.History ?? new())
            {
                var gameDataResponse = await client.GetAsync($"/match-details/v1/matches/{game.MatchID}");
                var gameData = await gameDataResponse.Content.ReadFromJsonAsync<ValorantMatch>();
                if (gameData is not null)
                    valorantMatches.Add(gameData);
            }

            if (valorantMatches is not null)
                _memoryCache.Set(cacheKey, valorantMatches);

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
