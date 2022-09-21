using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Models.RiotGames;
using AccountManager.Core.Models.RiotGames.Valorant;
using AccountManager.Core.Models.RiotGames.Valorant.Responses;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AccountManager.Core.Models.RiotGames.Requests;
using AccountManager.Core.Models.AppSettings;
using Microsoft.Extensions.Options;
using AutoMapper;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;

namespace AccountManager.Infrastructure.Clients
{
    public class RiotClient : IRiotClient
    {
        private readonly ILogger<RiotClient> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMapper _autoMapper;
        private readonly IRiotTokenClient _riotTokenClient;
        private readonly RiotTokenRequest valorantRequest = new RiotTokenRequest
        {
            Id = "play-valorant-web-prod",
            Nonce = "1",
            RedirectUri = "https://playvalorant.com/opt_in",
            ResponseType = "token id_token",
            Scope = "account openid"
        };
        private readonly RiotTokenRequest leagueRequest = new RiotTokenRequest
            {
                Id = "lol",
                Nonce = "1",
                RedirectUri = "http://localhost/redirect",
                ResponseType = "token id_token",
                Scope = "openid link ban lol_region"
            };
        public static readonly ImmutableDictionary<string, string> RiotAuthRegionMapping = (new Dictionary<string, string>()
            {
                {"na", "usw" },
                {"latam", "usw" },
                {"br", "usw" },
                {"eu", "euc" },
                {"ap", "apse" },
                {"kr", "apse" }
            }).ToImmutableDictionary();
        public RiotClient(IHttpClientFactory httpClientFactory, IOptions<RiotApiUri> riotApiOptions,
            IMapper autoMapper, IRiotTokenClient riotTokenClient, ILogger<RiotClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _autoMapper = autoMapper;
            _riotTokenClient = riotTokenClient;
            _logger = logger;
        }

        public async Task<string?> GetExpectedClientVersion()
        {
            var client = _httpClientFactory.CreateClient("Valorant3rdParty");
            try
            {
                var response = await client.GetFromJsonAsync<ExpectedClientVersionResponse>($"/v1/version");
                return response?.Data?.RiotClientVersion;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("Unable to get current riot client version! Message: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<string?> GetPuuId(Account account)
        {
            var riotTokens = await _riotTokenClient.GetRiotTokens(GetAuthRequest(account), account);
            if (string.IsNullOrEmpty(riotTokens.IdToken))
                return "";

            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(riotTokens.IdToken);
            jwtSecurityToken.Payload.TryGetValue("sub", out var puuid);

            if (puuid?.ToString() is null)
                _logger.LogError("Unable to get puuid for riot account! Account ID: {AccountId}, Account Username: {Username}", account.Id, account.Username);

            return puuid?.ToString() ?? "";
        }
      
        public async Task<RegionInfo> GetRegionInfo(Account account)
        {
            var riotTokens = await _riotTokenClient.GetRiotTokens(valorantRequest, account);
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri("https://riot-geo.pas.si.riotgames.com/");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", riotTokens.AccessToken);
            var affinityResponse = await client.PutAsJsonAsync<AffinityRequest>("/pas/v1/product/valorant", new() { IdToken = riotTokens.IdToken });
            
            try
            {
                affinityResponse.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("Unable to get region info from riot servers! Status Code: {StatusCode}, Message: {Message}, Account Username: {Username}", ex.StatusCode, ex.Message, account.Username);
                return _autoMapper.Map<RegionInfo>("na");
            }

            var affinityReponseJson = await affinityResponse.Content.ReadAsStringAsync();
            var afinities = JsonSerializer.Deserialize<AffinityResponse>(affinityReponseJson);

            return _autoMapper.Map<RegionInfo>(afinities?.Afinity?.Live ?? "na");
        }

        private RiotTokenRequest GetAuthRequest(Account account)
        {
            if (account.AccountType == Core.Enums.AccountType.League || account.AccountType == Core.Enums.AccountType.TeamFightTactics)
                return leagueRequest;
            else
                return valorantRequest;
        }
    }
}
