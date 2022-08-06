using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Models.RiotGames.Requests;
using AccountManager.Core.Models.RiotGames.Valorant;
using AccountManager.Core.Models.RiotGames.Valorant.Responses;
using AccountManager.Core.Models.UserSettings;
using AutoMapper;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace AccountManager.Infrastructure.Clients
{
    public class ValorantClient : IValorantClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IUserSettingsService<GeneralSettings> _settings;
        private readonly IRiotClient _riotClient;
        private readonly IMapper _autoMapper;
        private const int historyLength = 15;
        public ValorantClient(IHttpClientFactory httpClientFactory,
            IUserSettingsService<GeneralSettings> settings,
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

            var responseUri = new Uri(riotAuthResponse.Content.Response.Parameters.Uri);

            var queryString = responseUri.Fragment[1..];
            var queryDictionary = System.Web.HttpUtility.ParseQueryString(queryString);

            var token = queryDictionary["access_token"];

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

        public async Task<List<ValorantSkinLevelResponse>> GetValorantShopDeals(Account account)
        {

            var valClient = _httpClientFactory.CreateClient("ValorantNA");
            valClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Riot-ClientVersion", await _riotClient.GetExpectedClientVersion());
            var bearerToken = await GetValorantToken(account);
            if (bearerToken is null)
                return new();

            var entitlementToken = await _riotClient.GetEntitlementToken(bearerToken);

            valClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            valClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Riot-Entitlements-JWT", entitlementToken);

            var response = await valClient.GetFromJsonAsync<ValorantShopOffers>($"/store/v2/storefront/{account.PlatformId}");

            if (response?.SkinsPanelLayout?.SingleItemOffers is null)
                return new();

            var getSkinTasks = response.SkinsPanelLayout.SingleItemOffers.Select((offer) =>
            {
                return GetSkinFromUuid(offer);
            }).ToList();

            var tasks = new List<Task>();
            var allOffersTask = GetAllShopOffers(account);

            tasks.AddRange(getSkinTasks);
            tasks.Add(allOffersTask);

            await Task.WhenAll(tasks);

            var allOffers = allOffersTask.Result;

            var offers = getSkinTasks.Select((task) =>
            {
                var offer = task.Result;
                offer.Data.Price = allOffers?.Offers?.FirstOrDefault(allOffer => allOffer?.OfferID == offer.Data.Uuid)?.Cost._85ad13f73d1b51289eb27cd8ee0b5741 ?? 0;

                return offer;
            }).ToList();

            return offers;
        }

        public async Task<IEnumerable<ValorantMatch>?> GetValorantGameHistory(Account account)
        {
            var client = _httpClientFactory.CreateClient("ValorantNA");
            var expectedVersion = await _riotClient.GetExpectedClientVersion();
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-Riot-ClientVersion", expectedVersion);
            var bearerToken = await GetValorantToken(account);
            if (bearerToken is null)
                return Enumerable.Empty<ValorantMatch>();

            var entitlementToken = await _riotClient.GetEntitlementToken(bearerToken);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-Riot-Entitlements-JWT", entitlementToken);

            var gameHistoryData = await client.GetFromJsonAsync<ValorantGameHistoryDataResponse>($"/match-history/v1/history/{account.PlatformId}?queue=competitive&startIndex=0&endIndex={historyLength}");

            if (gameHistoryData?.History is null)
                return Enumerable.Empty<ValorantMatch>();

            var getValorantMatchesTasks = gameHistoryData.History.Select((game) =>
            {
                return client.GetFromJsonAsync<ValorantMatch>($"/match-details/v1/matches/{game.MatchID}");
            }).ToList();

            await Task.WhenAll(getValorantMatchesTasks);

            var valorantMatches = getValorantMatchesTasks.Select((gameDataResponseTask) =>
            {
                return gameDataResponseTask.Result ?? new();
            });

            if (valorantMatches is null)
                return Enumerable.Empty<ValorantMatch>();

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

        public async Task<ValorantOperatorsResponse> GetValorantOperators()
        {
            var client = _httpClientFactory.CreateClient("Valorant");
            var operatorsRequest = await client.GetAsync("/v1/agents");
            operatorsRequest.EnsureSuccessStatusCode();

            return await operatorsRequest.Content.ReadFromJsonAsync<ValorantOperatorsResponse>() ?? new();
        }

        private async Task<ValorantSkinLevelResponse> GetSkinFromUuid(string uuid)
        {
            var client = _httpClientFactory.CreateClient("Valorant");
            return await client.GetFromJsonAsync<ValorantSkinLevelResponse>($"/v1/weapons/skinlevels/{uuid}") ?? new();
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

            var offers = await client.GetFromJsonAsync<ValorantStoreTotalOffers>($"/store/v1/offers/");

            return offers;
        }
    }
}
