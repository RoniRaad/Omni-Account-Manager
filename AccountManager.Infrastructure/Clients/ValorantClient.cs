using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Models.RiotGames;
using AccountManager.Core.Models.RiotGames.Requests;
using AccountManager.Core.Models.RiotGames.Valorant;
using AccountManager.Core.Models.RiotGames.Valorant.Responses;
using AccountManager.Core.Models.UserSettings;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace AccountManager.Infrastructure.Clients
{
    public sealed class ValorantClient : IValorantClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<RiotTokenClient> _logger;
        private readonly IUserSettingsService<GeneralSettings> _settings;
        private readonly IRiotClient _riotClient;
        private readonly IRiotTokenClient _riotTokenClient;
        private readonly IRiotThirdPartyClient _riot3rdPartyClient;
        private readonly IMapper _autoMapper;
        private readonly RiotTokenRequest tokenRequest = new RiotTokenRequest
            {
                Id = "play-valorant-web-prod",
                Nonce = "1",
                RedirectUri = "https://playvalorant.com/opt_in",
                ResponseType = "token id_token",
                Scope = "account openid"
            };
        private const int historyLength = 15;
        public ValorantClient(IHttpClientFactory httpClientFactory,
            IUserSettingsService<GeneralSettings> settings,
            IRiotClient riotClient, IMapper autoMapper, 
            IRiotTokenClient riotTokenClient, ILogger<RiotTokenClient> logger, 
            IRiotThirdPartyClient riot3rdPartyClient)
        {
            _httpClientFactory = httpClientFactory;
            _httpClientFactory = httpClientFactory;
            _settings = settings;
            _riotClient = riotClient;
            _autoMapper = autoMapper;
            _riotTokenClient = riotTokenClient;
            _logger = logger;
            _riot3rdPartyClient = riot3rdPartyClient;
        }

        public async Task<ValorantRankedHistoryResponse?> GetValorantCompetitiveHistory(Account account)
        {
            var region = await _riotClient.GetValorantRegionInfo(account);
            var client = _httpClientFactory.CreateClient($"Valorant{region.RegionId.ToUpper()}");
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-Riot-ClientVersion", await _riotClient.GetExpectedClientVersion());
            var riotTokens = await _riotTokenClient.GetRiotTokens(tokenRequest, account);
            var entitlementToken = await _riotTokenClient.GetEntitlementToken(riotTokens.AccessToken);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", riotTokens.AccessToken);
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-Riot-Entitlements-JWT", entitlementToken);

            try
            {
                var response = await client.GetAsync($"/mmr/v1/players/{account.PlatformId}/competitiveupdates?queue=competitive&startIndex=0&endIndex={historyLength}");
                var rankedHistory = await response.Content.ReadFromJsonAsync<ValorantRankedHistoryResponse>();
                return rankedHistory;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("Unable to get expected valorant competitive history! Status Code: {StatusCode}, Message: {Message}, Account Username: {Username}", ex.StatusCode, ex.Message, account.Username);
                throw;
            }


        }

        public async Task<List<ValorantSkinLevelResponse>> GetValorantShopDeals(Account account)
        {
            ValorantShopOffers? shopOffers;
            var region = await _riotClient.GetValorantRegionInfo(account);
            var valClient = _httpClientFactory.CreateClient($"Valorant{region.RegionId.ToUpper()}");
            valClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Riot-ClientVersion", await _riotClient.GetExpectedClientVersion());
            var riotTokens = await _riotTokenClient.GetRiotTokens(tokenRequest, account);

            var entitlementToken = await _riotTokenClient.GetEntitlementToken(riotTokens.AccessToken);

            valClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", riotTokens.AccessToken);
            valClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Riot-Entitlements-JWT", entitlementToken);

            try
            {
                shopOffers = await valClient.GetFromJsonAsync<ValorantShopOffers>($"/store/v2/storefront/{account.PlatformId}");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("Unable to get valorant shop history! Status Code: {StatusCode}, Message: {Message}, Account Username: {Username}", ex.StatusCode, ex.Message, account.Username);
                throw;
            }

            if (shopOffers?.SkinsPanelLayout?.SingleItemOffers is null)
                return new();

            var getSkinTasks = shopOffers.SkinsPanelLayout.SingleItemOffers.Select((offer) =>
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
            ValorantGameHistoryDataResponse? gameHistoryData;
            var region = await _riotClient.GetValorantRegionInfo(account);
            var client = _httpClientFactory.CreateClient($"Valorant{region.RegionId.ToUpper()}");
            var expectedVersion = await _riotClient.GetExpectedClientVersion();
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-Riot-ClientVersion", expectedVersion);
            var riotTokens = await _riotTokenClient.GetRiotTokens(tokenRequest, account);
            var entitlementToken = await _riotTokenClient.GetEntitlementToken(riotTokens.AccessToken);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", riotTokens.AccessToken);
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-Riot-Entitlements-JWT", entitlementToken);

            try
            {
                gameHistoryData = await client.GetFromJsonAsync<ValorantGameHistoryDataResponse>($"/match-history/v1/history/{account.PlatformId}?queue=competitive&startIndex=0&endIndex={historyLength}");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("Unable to get valorant match history! Status Code: {StatusCode}, Message: {Message}", ex.StatusCode, ex.Message);
                throw;
            }

            if (gameHistoryData?.History is null)
                return Enumerable.Empty<ValorantMatch>();

            var getValorantMatchesTasks = gameHistoryData.History.Select((game) =>
            {
                try
                {
                    return client.GetFromJsonAsync<ValorantMatch>($"/match-details/v1/matches/{game.MatchID}");
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError("Unable to get a valorant match details! Status Code: {StatusCode}, Message: {Message}, Account Username: {Username}", ex.StatusCode, ex.Message, account.Username);
                    throw;
                }
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
            return await _riot3rdPartyClient.GetValorantOperators();
        }

        private async Task<ValorantSkinLevelResponse> GetSkinFromUuid(string uuid)
        {
            return await _riot3rdPartyClient.GetValorantSkinFromUuid(uuid);
        }

        private async Task<ValorantStoreTotalOffers?> GetAllShopOffers(Account account)
        {
            ValorantStoreTotalOffers? offers;
            var region = await _riotClient.GetValorantRegionInfo(account);
            var client = _httpClientFactory.CreateClient($"Valorant{region.RegionId.ToUpper()}");
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-Riot-ClientVersion", await _riotClient.GetExpectedClientVersion());
            var riotTokens = await _riotTokenClient.GetRiotTokens(tokenRequest, account);
            if (riotTokens is null)
                return new();

            var entitlementToken = await _riotTokenClient.GetEntitlementToken(riotTokens.AccessToken);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", riotTokens.AccessToken);
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-Riot-Entitlements-JWT", entitlementToken);

            try
            {
                offers = await client.GetFromJsonAsync<ValorantStoreTotalOffers>($"/store/v1/offers/");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("Unable to get valorant skin details! Status Code: {StatusCode}, Message: {Message}, Account Username: {Username}", ex.StatusCode, ex.Message, account.Username);
                throw;
            }

            return offers;
        }
    }
}
