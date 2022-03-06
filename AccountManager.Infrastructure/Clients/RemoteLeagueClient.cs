using AccountManager.Core.Enums;
using AccountManager.Core.Factories;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Models.League;
using AccountManager.Infrastructure.Services;
using CloudFlareUtilities;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using static AccountManager.Infrastructure.Clients.LocalLeagueClient;

namespace AccountManager.Infrastructure.Clients
{
    public class RemoteLeagueClient : ILeagueClient
    {
        private string token;
        private string port;
        private IMemoryCache _memoryCache;
        private IHttpClientFactory _httpClientFactory;
        private readonly ITokenService _leagueTokenService;
        private readonly LocalLeagueClient _localLeagueClient;
        private readonly HttpClient _httpClient;
        public RemoteLeagueClient(IMemoryCache memoryCache, IHttpClientFactory httpClientFactory, GenericFactory<AccountType, ITokenService> tokenFactory, LocalLeagueClient localLeagueClient)
        {
            _memoryCache = memoryCache;
            _leagueTokenService = tokenFactory.CreateImplementation(AccountType.League);
            var handler = new ClearanceHandler
            {
                MaxRetries = 2 // Optionally specify the number of retries, if clearance fails (default is 3).
            };
            _httpClientFactory = httpClientFactory;
            _localLeagueClient = localLeagueClient;
            _httpClient = httpClientFactory.CreateClient("CloudflareBypass");
        }

        public async Task<string> GetRankByUsernameAsync(string username)
        {
            if (!_leagueTokenService.TryGetPortAndToken(out token, out port))
                return "";

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"riot:{token}")));
            var response = await _httpClient.GetAsync($"https://127.0.0.1:{port}/lol-summoner/v1/summoners?name={username}");
            var summoner = await response.Content.ReadFromJsonAsync<LeagueAccount>();
            var rankResponse = await _httpClient.GetAsync($"https://127.0.0.1:{port}/lol-ranked/v1/ranked-stats/{summoner.Puuid}");
            var summonerRanking = await rankResponse.Content.ReadFromJsonAsync<LeagueSummonerRank>();
            return $"{summonerRanking.QueueMap.RankedSoloDuoStats.Tier} {summonerRanking.QueueMap.RankedSoloDuoStats.Division}";
        }
        public async Task<Rank> GetRankByPuuidAsync(Account account)
        {
            if (_localLeagueClient.IsClientOpen())
                return await _localLeagueClient.GetRankByPuuidAsync(account);

            var sessionToken = await GetLeagueSessionToken(account);
            var handler = new ClearanceHandler
            {
                MaxRetries = 2 // Optionally specify the number of retries, if clearance fails (default is 3).
            };

            // Create a HttpClient that uses the handler to bypass CloudFlare's JavaScript challange.
            var client = new HttpClient(handler);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessionToken);
            var rankResponse = await client.GetFromJsonAsync<LeagueRankedResponse>($"https://na-blue.lol.sgp.pvp.net/leagues-ledge/v2/rankedStats/puuid/{account.Id}");
            var rankedStats = rankResponse.Queues.Find((match) => match.QueueType == "RANKED_SOLO_5x5");

            return new Rank() { 
                Tier = rankedStats.Tier,
                Ranking = rankedStats.Rank,
            };
        }
        private async Task<string> GetRiotAuthToken(Account account)
        {
            var handler = new ClearanceHandler
            {
                MaxRetries = 2
            };

            using (var client = new HttpClient(handler))
            {
                var initialAuthTokenResponse = await client.PostAsJsonAsync("https://auth.riotgames.com/api/v1/authorization", new InitialAuthTokenRequest
                {
                    Id = "lol",
                    Nonce = "1",
                    RedirectUri = "http://localhost/redirect",
                    ResponseType = "token id_token",
                    Scope = "openid link ban lol_region"
                });
                initialAuthTokenResponse.EnsureSuccessStatusCode();

                var finalAuthRequest = await client.PutAsJsonAsync("https://auth.riotgames.com/api/v1/authorization", new FinalAuthTokenRequest
                {
                    Type = "auth",
                    Username = account.Username,
                    Password = account.Password,
                    Region = null
                });
                finalAuthRequest.EnsureSuccessStatusCode();

                var riotTokenResponse = await finalAuthRequest.Content.ReadFromJsonAsync<RiotAuthTokenWrapper>();

                var matches = Regex.Matches(riotTokenResponse.Response.Parameters.Uri,
                    @"access_token=((?:[a-zA-Z]|\d|\.|-|_)*).*id_token=((?:[a-zA-Z]|\d|\.|-|_)*).*expires_in=(\d*)");
                token = matches[0].Groups[1].Value;// Cache
                return token;
            }
        }
        // Requires RiotAuthToken header
        private async Task<string> GetUserInfo(Account account, string riotToken)
        {
            string userInfo;

            _httpClient.DefaultRequestHeaders.Authorization = new("Bearer", riotToken);
            var userInfoResponse = await _httpClient.GetAsync("https://auth.riotgames.com/userinfo");
            userInfoResponse.EnsureSuccessStatusCode();
            userInfo = await userInfoResponse.Content.ReadAsStringAsync();

            return userInfo;
        }
        private async Task<string> GetEntitlementJWT(Account account, string riotToken)
        {
            string entitlement;
            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            _httpClient.DefaultRequestHeaders.Authorization = new("Bearer", riotToken);

            var entitlementResponse = await _httpClient.PostAsJsonAsync("https://entitlements.auth.riotgames.com/api/token/v1", new { urn = "urn:entitlement" });
            entitlementResponse.EnsureSuccessStatusCode();
            var entitlementResponseDeserialized = await entitlementResponse.Content.ReadFromJsonAsync<EntitlementResponse>();
            entitlement = entitlementResponseDeserialized.EntitlementToken;

            return entitlement;
        }
        // Requires RiotAuthToken header
        private async Task<string> GetLeagueLoginToken(Account account)
        {
            var riotToken = await GetRiotAuthToken(account);
            var userInfo = await GetUserInfo(account, riotToken);
            var entitlement = await GetEntitlementJWT(account, riotToken);

            _httpClient.DefaultRequestHeaders.Authorization = new("Bearer", riotToken);

            var loginResponse = await _httpClient.PostAsJsonAsync($"https://usw.pp.riotgames.com/login-queue/v2/login/products/lol/regions/na1", new LoginRequest
            {
                Entitlements = entitlement,
                UserInfo = userInfo
            });
            var str = loginResponse.Content.ReadAsStringAsync();
            loginResponse.EnsureSuccessStatusCode();
            var tokenResponse = await loginResponse.Content.ReadFromJsonAsync<TokenResponse>();
            token = tokenResponse.Token;

            return token;
        }
        // Requires LeagueLoginToken header
        public async Task<string> CreateLeagueSession(Account account)
        {
            string sessionToken;

            var puuId = account.Id;
            var leagueToken = await GetLeagueLoginToken(account);

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new("Bearer", leagueToken);

            var sessionResponse = await client.PostAsJsonAsync($"https://usw.pp.riotgames.com/session-external/v1/session/create", new PostSessionsRequest
            {
                Claims = new Claims
                {
                    CName = "lcu"
                },
                Product = "lol",
                PuuId = puuId,
                Region = "NA1"
            });
            var str = sessionResponse.Content.ReadAsStringAsync();
            sessionResponse.EnsureSuccessStatusCode();
            sessionToken = await sessionResponse.Content.ReadAsStringAsync();

            return sessionToken.Replace("\"", "");
        }
        public async Task<string> GetLocalSessionToken()
        {
            if (!_leagueTokenService.TryGetPortAndToken(out string token, out string port))
                return string.Empty;

            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
            {
                return true;
            };

            // TODO: Inject this client
            HttpClient client = new HttpClient(httpClientHandler);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"riot:{token}")));
            var rankResponse = await client.GetFromJsonAsync<LeagueSessionResponse>($"https://127.0.0.1:{port}/lol-login/v2/league-session-init-token");
            return rankResponse.Token;
        }
        public async Task<string> GetLeagueSessionToken(Account account)
        {
            if (_memoryCache.TryGetValue<string>("GetLeagueSessionToken", out string sessionToken))
                if (await TestLeagueToken(sessionToken))
                    return sessionToken;

            sessionToken = await GetLocalSessionToken();
            if (string.IsNullOrEmpty(sessionToken))
                sessionToken = await CreateLeagueSession(account);

            _memoryCache.Set("GetLeagueSessionToken", sessionToken);
            return sessionToken;
        }

        public async Task<bool> TestLeagueToken(string token)
        {
            var handler = new ClearanceHandler
            {
                MaxRetries = 2 // Optionally specify the number of retries, if clearance fails (default is 3).
            };

            // Create a HttpClient that uses the handler to bypass CloudFlare's JavaScript challange.
            var client = new HttpClient(handler);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var rankResponse = await client.GetAsync($"https://na-blue.lol.sgp.pvp.net/leagues-ledge/v2/rankedStats/puuid/fakepuuid");
            return rankResponse.IsSuccessStatusCode;
        }
    }
    internal class TokenResponse
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }
    }
    internal class PostSessionsRequest
    {
        [JsonPropertyName("claims")]
        public Claims Claims { get; set; }
        [JsonPropertyName("product")]
        public string Product { get; set; }
        [JsonPropertyName("puuid")]
        public string PuuId { get; set; }
        [JsonPropertyName("region")]
        public string Region { get; set; }
    }
    internal class Claims
    {
        [JsonPropertyName("cname")]
        public string CName { get; set; }
    }
    internal class LoginRequest
    {
        [JsonPropertyName("clientName")]
        public string Name { get; set; } = "lcu";
        [JsonPropertyName("entitlements")]
        public string Entitlements { get; set; }
        [JsonPropertyName("userinfo")]
        public string UserInfo { get; set; }
    }
    internal class EntitlementResponse
    {
        [JsonPropertyName("entitlements_token")]
        public string EntitlementToken { get; set; }
    }
    internal class RiotAuthTokenWrapper
    {
        [JsonPropertyName("response")]
        public RiotAuthTokenResponse Response { get; set; }
    }
    public class RiotAuthTokenResponse
    {
        [JsonPropertyName("parameters")]
        public RiotAuthTokenParameters Parameters { get; set; }
    }
    public class RiotAuthTokenParameters
    {
        [JsonPropertyName("uri")]
        public string Uri { get; set; }
    }
    internal class InitialAuthTokenRequest
    {
        [JsonPropertyName("client_id")]
        public string Id { get; set; }
        [JsonPropertyName("nonce")]
        public string Nonce { get; set; }
        [JsonPropertyName("redirect_uri")]
        public string RedirectUri { get; set; }
        [JsonPropertyName("response_type")]
        public string ResponseType { get; set; }
        [JsonPropertyName("scope")]
        public string Scope { get; set; }
    }
    internal class FinalAuthTokenRequest
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("username")]
        public string Username { get; set; }
        [JsonPropertyName("password")]
        public string Password { get; set; }
        [JsonPropertyName("region")]
        public string Region { get; set; }
    }

    public class Queue
    {
        [JsonPropertyName("queueType")]
        public string QueueType { get; set; }

        [JsonPropertyName("provisionalGameThreshold")]
        public int ProvisionalGameThreshold { get; set; }

        [JsonPropertyName("tier")]
        public string Tier { get; set; }

        [JsonPropertyName("rank")]
        public string Rank { get; set; }

        [JsonPropertyName("leaguePoints")]
        public int LeaguePoints { get; set; }

        [JsonPropertyName("wins")]
        public int Wins { get; set; }

        [JsonPropertyName("losses")]
        public int Losses { get; set; }

        [JsonPropertyName("provisionalGamesRemaining")]
        public int ProvisionalGamesRemaining { get; set; }

        [JsonPropertyName("previousSeasonEndTier")]
        public string PreviousSeasonEndTier { get; set; }

        [JsonPropertyName("previousSeasonEndRank")]
        public string PreviousSeasonEndRank { get; set; }

        [JsonPropertyName("ratedRating")]
        public int RatedRating { get; set; }
    }

    public class SplitsProgress
    {
        [JsonPropertyName("1")]
        public int _1 { get; set; }
    }

    public class RANKEDTFT
    {
        [JsonPropertyName("currentSeasonId")]
        public int CurrentSeasonId { get; set; }

        [JsonPropertyName("currentSeasonEnd")]
        public long CurrentSeasonEnd { get; set; }

        [JsonPropertyName("nextSeasonStart")]
        public int NextSeasonStart { get; set; }
    }

    public class RANKEDTFTTURBO
    {
        [JsonPropertyName("currentSeasonId")]
        public int CurrentSeasonId { get; set; }

        [JsonPropertyName("currentSeasonEnd")]
        public long CurrentSeasonEnd { get; set; }

        [JsonPropertyName("nextSeasonStart")]
        public int NextSeasonStart { get; set; }
    }

    public class RANKEDTFTPAIRS
    {
        [JsonPropertyName("currentSeasonId")]
        public int CurrentSeasonId { get; set; }

        [JsonPropertyName("currentSeasonEnd")]
        public long CurrentSeasonEnd { get; set; }

        [JsonPropertyName("nextSeasonStart")]
        public int NextSeasonStart { get; set; }
    }

    public class RANKEDFLEXSR
    {
        [JsonPropertyName("currentSeasonId")]
        public int CurrentSeasonId { get; set; }

        [JsonPropertyName("currentSeasonEnd")]
        public long CurrentSeasonEnd { get; set; }

        [JsonPropertyName("nextSeasonStart")]
        public int NextSeasonStart { get; set; }
    }

    public class RANKEDSOLO5x5
    {
        [JsonPropertyName("currentSeasonId")]
        public int CurrentSeasonId { get; set; }

        [JsonPropertyName("currentSeasonEnd")]
        public long CurrentSeasonEnd { get; set; }

        [JsonPropertyName("nextSeasonStart")]
        public int NextSeasonStart { get; set; }
    }

    public class Seasons
    {
        [JsonPropertyName("RANKED_TFT")]
        public RANKEDTFT RANKEDTFT { get; set; }

        [JsonPropertyName("RANKED_TFT_TURBO")]
        public RANKEDTFTTURBO RANKEDTFTTURBO { get; set; }

        [JsonPropertyName("RANKED_TFT_PAIRS")]
        public RANKEDTFTPAIRS RANKEDTFTPAIRS { get; set; }

        [JsonPropertyName("RANKED_FLEX_SR")]
        public RANKEDFLEXSR RANKEDFLEXSR { get; set; }

        [JsonPropertyName("RANKED_SOLO_5x5")]
        public RANKEDSOLO5x5 RANKEDSOLO5x5 { get; set; }
    }

    public class LeagueRankedResponse
    {
        [JsonPropertyName("queues")]
        public List<Queue> Queues { get; set; }

        [JsonPropertyName("highestPreviousSeasonEndTier")]
        public string HighestPreviousSeasonEndTier { get; set; }

        [JsonPropertyName("highestPreviousSeasonEndRank")]
        public string HighestPreviousSeasonEndRank { get; set; }

        [JsonPropertyName("earnedRegaliaRewardIds")]
        public List<object> EarnedRegaliaRewardIds { get; set; }

        [JsonPropertyName("splitsProgress")]
        public SplitsProgress SplitsProgress { get; set; }

        [JsonPropertyName("seasons")]
        public Seasons Seasons { get; set; }
    }
}
