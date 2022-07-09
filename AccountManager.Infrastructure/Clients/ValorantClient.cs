using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Models.RiotGames.Requests;
using AccountManager.Core.Models.RiotGames.Valorant;
using AccountManager.Core.Models.RiotGames.Valorant.Responses;
using AutoMapper;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace AccountManager.Infrastructure.Clients
{
    public class ValorantClient : IValorantClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IUserSettingsService<UserSettings> _settings;
        private readonly IRiotClient _riotClient;
        private readonly IMapper _autoMapper;
        private const int historyLength = 15;
        public ValorantClient(IHttpClientFactory httpClientFactory,
            IUserSettingsService<UserSettings> settings,
            IRiotClient riotClient, IMapper autoMapper)
        {
            _httpClientFactory = httpClientFactory;
            _httpClientFactory = httpClientFactory;
            _settings = settings;
            _riotClient = riotClient;
            _autoMapper = autoMapper;
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

            var riotAuthResponse = await _riotClient.RiotAuthenticate(initialAuthTokenRequest, account);

            if (riotAuthResponse is null || riotAuthResponse?.Content?.Response?.Parameters?.Uri is null)
                return null;

            var matches = Regex.Matches(riotAuthResponse.Content.Response.Parameters.Uri,
                    @"access_token=((?:[a-zA-Z]|\d|\.|-|_)*).*id_token=((?:[a-zA-Z]|\d|\.|-|_)*).*expires_in=(\d*)");

            var token = matches[0].Groups[1].Value;

            return token;
        }

        public async Task<ValorantRankedHistoryResponse?> GetValorantCompetitiveHistory(Account account)
        {
            var client = _httpClientFactory.CreateClient("ValorantNA");
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-Riot-ClientVersion", await _riotClient.GetExpectedClientVersion());
            var bearerToken = await GetValorantToken(account);
            if (bearerToken is null)
                return new();

            var entitlementToken = await _riotClient.GetEntitlementToken(bearerToken);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-Riot-Entitlements-JWT", entitlementToken);

            var response = await client.GetAsync($"/mmr/v1/players/{account.PlatformId}/competitiveupdates?queue=competitive&startIndex=0&endIndex={historyLength}");
            var rankedHistory = await response.Content.ReadFromJsonAsync<ValorantRankedHistoryResponse>();

            return rankedHistory;
        }
        private async Task<ValorantSkinLevelResponse> GetSkinFromUuid(string uuid)
        {
            var client = _httpClientFactory.CreateClient();
            return await client.GetFromJsonAsync<ValorantSkinLevelResponse>($"https://valorant-api.com/v1/weapons/skinlevels/{uuid}") ?? new();
        }


        public async Task<List<ValorantSkinLevelResponse>> GetValorantShopDeals(Account account)
        {
            var offers = new List<ValorantSkinLevelResponse>();

            var valClient = _httpClientFactory.CreateClient("ValorantNA");
            valClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Riot-ClientVersion", await _riotClient.GetExpectedClientVersion());
            var bearerToken = await GetValorantToken(account);
            if (bearerToken is null)
                return new();

            var entitlementToken = await _riotClient.GetEntitlementToken(bearerToken);

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

            return offers;
        }

        public async Task<IEnumerable<ValorantMatch>?> GetValorantGameHistory(Account account)
        {
            var client = _httpClientFactory.CreateClient("ValorantNA");
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-Riot-ClientVersion", await _riotClient.GetExpectedClientVersion());
            var bearerToken = await GetValorantToken(account);
            if (bearerToken is null)
                return new List<ValorantMatch>();

            var entitlementToken = await _riotClient.GetEntitlementToken(bearerToken);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-Riot-Entitlements-JWT", entitlementToken);

            var gameHistoryDataResponse = await client.GetAsync($"/match-history/v1/history/{account.PlatformId}?queue=competitive&startIndex=0&endIndex={historyLength}");
            var gameHistoryData = await gameHistoryDataResponse.Content.ReadFromJsonAsync<ValorantGameHistoryDataResponse>();

            var valorantMatches = new List<ValorantMatch>();

            foreach (var game in gameHistoryData?.History ?? new())
            {
                var gameDataResponse = await client.GetAsync($"/match-details/v1/matches/{game.MatchID}");
                var gameData = await gameDataResponse.Content.ReadFromJsonAsync<ValorantMatch>();
                if (gameData is not null)
                    valorantMatches.Add(gameData);
            }

            return valorantMatches;
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

        private async Task<ValorantStoreTotalOffers?> GetAllShopOffers(Account account)
        {
            var client = _httpClientFactory.CreateClient("ValorantNA");
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-Riot-ClientVersion", await _riotClient.GetExpectedClientVersion());
            var bearerToken = await GetValorantToken(account);
            if (bearerToken is null)
                return new();

            var entitlementToken = await _riotClient.GetEntitlementToken(bearerToken);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-Riot-Entitlements-JWT", entitlementToken);

            var response = await client.GetAsync($"/store/v1/offers/");
            var offers = await response.Content.ReadFromJsonAsync<ValorantStoreTotalOffers>();

            return offers;
        }
    }
}
