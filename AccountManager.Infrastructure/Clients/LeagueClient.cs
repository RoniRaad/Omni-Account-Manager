using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Models.AppSettings;
using AccountManager.Core.Models.RiotGames.League;
using AccountManager.Core.Models.RiotGames.League.Requests;
using AccountManager.Core.Models.RiotGames.League.Responses;
using AccountManager.Core.Models.RiotGames.TeamFightTactics.Responses;
using AccountManager.Core.Models.UserSettings;
using AutoMapper;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace AccountManager.Infrastructure.Clients
{
    public class LeagueClient : ILeagueClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILeagueTokenClient _leagueTokenClient;
        private readonly IUserSettingsService<LeagueSettings> _settings;
        private readonly RiotApiUri _riotApiUri;
        private readonly IMapper _autoMapper;
        private const int historyLength = 15;
        public LeagueClient(IHttpClientFactory httpClientFactory,
            ILeagueTokenClient leagueTokenClient, IUserSettingsService<LeagueSettings> settings,
            IOptions<RiotApiUri> riotApiOptions, IMapper autoMapper)
        {
            _httpClientFactory = httpClientFactory;
            _leagueTokenClient = leagueTokenClient;
            _httpClientFactory = httpClientFactory;
            _settings = settings;
            _riotApiUri = riotApiOptions.Value;
            _autoMapper = autoMapper;
        }

        public async Task<Rank> GetSummonerRankByPuuidAsync(Account account)
        {

            if (!_settings.Settings.UseAccountCredentials)
                return new Rank();

            var queue = await GetRankQueuesByPuuidAsync(account);
            var rankedStats = queue.Find((match) => match.QueueType == "RANKED_SOLO_5x5");

            return _autoMapper.Map<LeagueRank>(new Rank()
            {
                Tier = rankedStats?.Tier,
                Ranking = rankedStats?.Rank,
            });
        }

        private async Task<List<Queue>> GetRankQueuesByPuuidAsync(Account account)
        {
            var sessionToken = await _leagueTokenClient.GetLeagueSessionToken();

            var client = _httpClientFactory.CreateClient("CloudflareBypass");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessionToken);
            
            var rankResponse = await client.GetFromJsonAsync<LeagueRankedResponse>($"{_riotApiUri.LeagueNA}/leagues-ledge/v2/rankedStats/puuid/{account.PlatformId}");

            if (rankResponse?.Queues is null)
                return new List<Queue>();

            return rankResponse.Queues;
        }

        public async Task<Rank> GetTFTRankByPuuidAsync(Account account)
        {

            if (!_settings.Settings.UseAccountCredentials)
                return new Rank();

            var queue = await GetRankQueuesByPuuidAsync(account);
            var rankedStats = queue.Find((match) => match.QueueType == "RANKED_TFT");
            if (rankedStats?.Tier?.ToLower() == "none" || rankedStats?.Tier is null)
                return _autoMapper.Map<TeamFightTacticsRank>(new Rank()
                {
                    Tier = "UNRANKED",
                    Ranking = ""
                });

            return _autoMapper.Map<TeamFightTacticsRank>(new Rank()
            {
                Tier = rankedStats?.Tier,
                Ranking = rankedStats?.Rank,
            });
        }

        public async Task<List<LeagueQueueMapResponse>?> GetLeagueQueueMappings()
        {
            var client = _httpClientFactory.CreateClient();
            var queueMapping = await client.GetFromJsonAsync<List<LeagueQueueMapResponse>>($"{_riotApiUri.RiotCDN}/docs/lol/queues.json");
            return queueMapping;
        }

        private async Task<MatchHistoryResponse?> GetLeagueMatchHistory(Account account)
        {
            if (!_settings.Settings.UseAccountCredentials)
                return new();

            var token = await _leagueTokenClient.GetLeagueSessionToken();
            var client = _httpClientFactory.CreateClient("CloudflareBypass");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var matchHistory = await client.GetFromJsonAsync<MatchHistoryResponse>($"{_riotApiUri.LeagueSessionUS}/match-history-query/v1/products/lol/player/{account.PlatformId}/SUMMARY?startIndex=0&count={historyLength}");

            return matchHistory;
        }

        public async Task<UserChampSelectHistory?> GetUserChampSelectHistory(Account account)
        {
            var rankResponse = await GetUserLeagueMatchHistory(account);
            var userInGames = rankResponse?.Games?.Select((game) => game?.Json?.Participants?.FirstOrDefault((participants) => participants?.Puuid == account.PlatformId, null));
            var selectedChampGroup = userInGames?.GroupBy((userInGame) => userInGame?.ChampionName);
            var matchHistory =  new UserChampSelectHistory()
            {
                Champs = selectedChampGroup?.Select((grouping) => new ChampSelectedCount()
                {
                    ChampName = grouping?.Key ?? "Unknown",
                    SelectedCount = grouping?.Count() ?? 0
                }) ?? new List<ChampSelectedCount>()
            };

            return matchHistory;
        }

        public async Task<MatchHistory?> GetUserLeagueMatchHistory(Account account)
        {
            var rankResponse = await GetLeagueMatchHistory(account);
            if (rankResponse is null)
                return null;

            var matchHistory = _autoMapper.Map<MatchHistory>(rankResponse);

            return matchHistory;
        }

        public async Task<TeamFightTacticsMatchHistory?> GetUserTeamFightTacticsMatchHistory(Account account)
        {
            if (!_settings.Settings.UseAccountCredentials)
                return new();

            var token = await _leagueTokenClient.GetLeagueSessionToken();
            var client = _httpClientFactory.CreateClient("CloudflareBypass");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var rankResponse = await client.GetFromJsonAsync<TeamFightTacticsMatchHistory>($"{_riotApiUri.LeagueSessionUS}/match-history-query/v1/products/tft/player/{account.PlatformId}/SUMMARY?startIndex=0&count={historyLength}");
            
            return rankResponse;
        }
    }
}
