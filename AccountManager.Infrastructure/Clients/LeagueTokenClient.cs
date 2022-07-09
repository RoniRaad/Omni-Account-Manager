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
using AccountManager.Core.Factories;

namespace AccountManager.Infrastructure.Clients
{
    public class LeagueTokenClient : ILeagueTokenClient
    {
        private readonly ITokenService _leagueTokenService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IRiotClient _riotClient;
        private readonly RiotApiUri _riotApiUri;
        private readonly ICurlRequestBuilder _curlRequestBuilder;
        public LeagueTokenClient(IHttpClientFactory httpClientFactory,
            IRiotClient riotClient, IOptions<RiotApiUri> riotApiOptions,
            ICurlRequestBuilder curlRequestBuilder, GenericFactory<AccountType, ITokenService> leagueTokenServiceFactory)
        {
            _httpClientFactory = httpClientFactory;
            _httpClientFactory = httpClientFactory;
            _riotClient = riotClient;
            _riotApiUri = riotApiOptions.Value;
            _curlRequestBuilder = curlRequestBuilder;
            _leagueTokenService = leagueTokenServiceFactory.CreateImplementation(AccountType.League);
        }

        public async Task<string> GetLocalSessionToken()
        {
            if (!_leagueTokenService.TryGetPortAndToken(out string token, out string port))
                return string.Empty;
            var client = _httpClientFactory.CreateClient("SSLBypass");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"riot:{token}")));
            var rankResponse = await client.GetFromJsonAsync<string>($"https://127.0.0.1:{port}/lol-league-session/v1/league-session-token");
            if (rankResponse is null)
                return string.Empty;

            return rankResponse;
        }

        public async Task<string> GetLeagueSessionToken(Account account)
        {
            try
            {
                var sessionToken = await GetLocalSessionToken();
                if (string.IsNullOrEmpty(sessionToken) || !await TestLeagueToken(sessionToken))
                    sessionToken = await CreateLeagueSession(account);

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

        private async Task<string> GetLeagueLoginToken(Account account)
        {
            string token;
            var riotToken = await GetRiotAuthToken(account);
            var userInfo = await GetUserInfo(riotToken ?? "");
            var entitlement = await GetEntitlementJWT(riotToken ?? "");
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

        private async Task<string?> GetRiotAuthToken(Account account)
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

            var responseUri = new Uri(response.Content.Response.Parameters.Uri);

            var queryString = responseUri.Fragment[1..];
            var queryDictionary = System.Web.HttpUtility.ParseQueryString(queryString);

            var token = queryDictionary["access_token"];

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
    }
}
