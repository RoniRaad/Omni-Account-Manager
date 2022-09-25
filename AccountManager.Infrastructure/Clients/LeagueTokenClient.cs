using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using System.Net.Http.Json;
using AccountManager.Core.Models.AppSettings;
using System.Net.Http.Headers;
using AccountManager.Core.Models.RiotGames.League.Requests;
using AccountManager.Core.Models.RiotGames.League;
using System.Net;
using AccountManager.Core.Models.RiotGames.League.Responses;
using AccountManager.Core.Models.RiotGames.Requests;
using Microsoft.Extensions.Options;
using AccountManager.Core.Enums;
using AccountManager.Core.Models.UserSettings;
using AutoMapper;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace AccountManager.Infrastructure.Clients
{
    public sealed class LeagueTokenClient : ILeagueTokenClient
    {
        private readonly ITokenService _leagueTokenService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IRiotClient _riotClient;
        private readonly IRiotTokenClient _riotTokenClient;
        private readonly RiotApiUri _riotApiUri;
        private readonly IHttpRequestBuilder _curlRequestBuilder;
        private readonly IUserSettingsService<LeagueSettings> _leagueSettings;
        private readonly IAppState _state;
        private readonly ILogger<LeagueTokenClient> _logger;
        private readonly RiotTokenRequest riotTokenRequest = new()
        {
            Id = "lol",
            Nonce = "1",
            RedirectUri = "http://localhost/redirect",
            ResponseType = "token id_token",
            Scope = "openid link ban lol_region"
        };

        public LeagueTokenClient(IHttpClientFactory httpClientFactory,
            IRiotClient riotClient, IOptions<RiotApiUri> riotApiOptions,
            IHttpRequestBuilder curlRequestBuilder, IGenericFactory<AccountType, ITokenService> leagueTokenServiceFactory,
            IUserSettingsService<LeagueSettings> leagueSettings, IAppState state, IRiotTokenClient riotTokenClient, ILogger<LeagueTokenClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _httpClientFactory = httpClientFactory;
            _riotClient = riotClient;
            _riotApiUri = riotApiOptions.Value;
            _curlRequestBuilder = curlRequestBuilder;
            _leagueTokenService = leagueTokenServiceFactory.CreateImplementation(AccountType.League);
            _leagueSettings = leagueSettings;
            _state = state;
            _riotTokenClient = riotTokenClient;
            _logger = logger;
        }

        public async Task<string> GetLocalSessionToken()
        {
            if (!_leagueTokenService.TryGetPortAndToken(out string token, out string port))
                return string.Empty;
            var client = _httpClientFactory.CreateClient("SSLBypass");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"riot:{token}")));
            var sessionTokenResponse = await client.GetAsync($"https://127.0.0.1:{port}/lol-league-session/v1/league-session-token");
            try
            {
                sessionTokenResponse.EnsureSuccessStatusCode();
            }
            catch(HttpRequestException ex)
            {
                _logger.LogError("Unable to get local league of legends session token! Status Code: {StatusCode}, Message: {Message}", ex.StatusCode, ex.Message);
                return string.Empty;
            }
            
            var sessionToken = await sessionTokenResponse.Content.ReadAsStringAsync();
            if (sessionToken is null)
            {
                _logger.LogError("Unable to get local league of legends session token! Token was null!");
                return string.Empty;
            }

            return sessionToken.Trim('\"');
        }

        public async Task<string> GetLeagueSessionToken()
        {
            try
            {
                var sessionToken = await GetLocalSessionToken();
                if (string.IsNullOrEmpty(sessionToken) || !await TestLeagueToken(sessionToken))
                    sessionToken = await CreateLeagueSession();

                return sessionToken;
            }
            catch
            {
                return string.Empty;
            }
        }

        public async Task<bool> TestLeagueToken(string token)
        {
            var client = _httpClientFactory.CreateClient("CloudflareBypass");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var rankResponse = await client.GetAsync($"{_riotApiUri.LeagueNA}/leagues-ledge/v2/rankedStats/puuid/fakepuuid");
            return rankResponse.IsSuccessStatusCode;
        }

        public async Task<string> CreateLeagueSession()
        {
            string sessionToken;
            Account? account;
            if (_leagueSettings?.Settings?.AccountToUseCredentials is null)
            {
                account = _state.Accounts.FirstOrDefault((acc) => acc.AccountType == AccountType.League);
            }
            else
            {
                account = _state.Accounts.FirstOrDefault((acc) => acc.Guid == _leagueSettings.Settings.AccountToUseCredentials);
            }

            if ( account is null )
                return string.Empty;

            var puuId = account?.PlatformId ?? await _riotClient.GetPuuId(account);

            if (puuId is null)
                return string.Empty;

            var leagueToken = await GetLeagueLoginToken(account);
            if (string.IsNullOrEmpty(leagueToken))
                return string.Empty;

            var region = await _riotClient.GetRegionInfo(account);
            var client = _httpClientFactory.CreateClient($"LeagueSession{region.CountryId.ToUpper()}");

            client.DefaultRequestHeaders.Authorization = new("Bearer", leagueToken);
            JwtSecurityTokenHandler jwtSecurityTokenHandler = new();
            var jwtToken = jwtSecurityTokenHandler.ReadJwtToken(leagueToken);
            jwtToken.Payload.TryGetValue("region", out object? regionCode);

            regionCode ??= "NA1";

            var sessionResponse = await client.PostAsJsonAsync($"/session-external/v1/session/create", new PostSessionsRequest
            {
                Claims = new Claims
                {
                    CName = "lcu"
                },
                Product = "lol",
                PuuId = puuId,
                Region = regionCode.ToString()
            });

            try
            {
                sessionResponse.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("Unable to create league of legends session with account username {Username}! Status Code: {StatusCode}, Message: {Message}", account.Username, ex.StatusCode, ex.Message);
                return string.Empty;
            }

            sessionToken = await sessionResponse.Content.ReadAsStringAsync();

            return sessionToken.Replace("\"", "");
        }

        private async Task<string> GetLeagueLoginToken(Account account)
        {
            string token;
            var riotToken = await _riotTokenClient.GetRiotTokens(riotTokenRequest, account);
            var userInfo = await GetUserInfo(riotToken.AccessToken ?? "");
            var entitlement = await GetEntitlementJWT(riotToken.AccessToken ?? "");
            if (string.IsNullOrEmpty(riotToken.AccessToken)
                || string.IsNullOrEmpty(userInfo)
                || string.IsNullOrEmpty(entitlement))
                return string.Empty;

            var region = await _riotClient.GetRegionInfo(account);
            var client = _httpClientFactory.CreateClient($"LeagueSession{region.CountryId.ToUpper()}");

            client.DefaultRequestHeaders.Authorization = new("Bearer", riotToken.AccessToken);
            var leagueInfo = GetLeagueInfo(riotToken.IdToken);
            var countryId = leagueInfo?.Pid ?? "na1";

            var loginResponse = await client.PostAsJsonAsync($"/login-queue/v2/login/products/lol/regions/{countryId}", new LoginRequest
            {
                Entitlements = entitlement,
                UserInfo = userInfo
            });

            try
            {
                loginResponse.EnsureSuccessStatusCode();

            }
            catch (HttpRequestException ex) 
            {
                _logger.LogError("Could not get remote league of legends login token! Status Code: {StatusCode}, Message: {Message}", ex.StatusCode, ex.Message);
            }
            var tokenResponse = await loginResponse.Content.ReadFromJsonAsync<TokenResponse>();
            if (tokenResponse?.Token is null)
                return string.Empty;

            token = tokenResponse.Token;

            return token;
        }

        private async Task<string> GetUserInfo(string riotToken)
        {
            var response = await _curlRequestBuilder.CreateBuilder()
            .SetUri($"{_riotApiUri.Auth}/userinfo")
            .SetBearerToken(riotToken)
            .SetUserAgent(_riotApiUri.UserAgent)
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
            .SetUserAgent(_riotApiUri.UserAgent)
            .Post<EntitlementResponse>();

            entitlement = response?.ResponseContent?.EntitlementToken ?? "";

            return entitlement;
        }

        private LeagueIdInfo? GetLeagueInfo(string idToken)
        {
            JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            var parsedIdToken = jwtSecurityTokenHandler.ReadJwtToken(idToken);
            parsedIdToken.Payload.TryGetValue("lol", out object? leagueInfoJson);
            if (leagueInfoJson?.ToString() is null)
                return null;

            var leagueInfo = JsonSerializer.Deserialize<List<LeagueIdInfo>>(leagueInfoJson?.ToString() ?? "{}");

            return leagueInfo?.FirstOrDefault();
        }
    }
}
