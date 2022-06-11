using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Models.RiotGames.Valorant;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;

namespace AccountManager.Core.Services.GraphServices
{
    public class ValorantGraphService : IValorantGraphService
    {
        private readonly IRiotClient _riotClient;
        private readonly AlertService _alertService;
        private readonly IMemoryCache _memoryCache;
        private readonly IMapper _mapper;

        public ValorantGraphService(IRiotClient riotClient, AlertService alertService,
            IMemoryCache memoryCache, IMapper mapper, IUserSettingsService<UserSettings> settingsService)
        {
            _riotClient = riotClient;
            _alertService = alertService;
            _memoryCache = memoryCache;
            _mapper = mapper;
        }
        public async Task<LineGraph> GetRankedWinsLineGraph(Account account)
        {
            var matchHistory = await _riotClient.GetValorantGameHistory(account, 0, 15);
            if (matchHistory?.Any() is not true)
                return new LineGraph();
            matchHistory = matchHistory.OrderBy(match => match.MatchInfo.GameStartMillis);
            Dictionary<int, RankedGraphData> rankedWins = new Dictionary<int, RankedGraphData>();
            Dictionary<int, int> rankedOffsets = new Dictionary<int, int>();
            int previousTier = (int)matchHistory.First().Players.First(player => player.Subject == account.PlatformId).CompetitiveTier;
            for (var i = 0; i < matchHistory.Count(); i++)
            {
                var currentMatch = matchHistory.ElementAt(i);
                var accountInMatch = currentMatch.Players.First(player => player.Subject == account.PlatformId);
                var accountTeam = currentMatch.Teams.First(team => team.TeamId == accountInMatch.TeamId);
                var currentTier = (int)accountInMatch.CompetitiveTier;

                if (!rankedOffsets.TryGetValue((int)accountInMatch.CompetitiveTier, out var offset))
                    rankedOffsets[currentTier] = 0;

                if (!rankedWins.TryGetValue((int)accountInMatch.CompetitiveTier, out var valorantMatches) || valorantMatches == null)
                {
                    valorantMatches = new RankedGraphData();
                    rankedWins.Add(currentTier, valorantMatches);
                }
                if (previousTier != currentTier)
                    rankedWins[previousTier].Data.Add(new CoordinatePair() { X = null, Y = null });

                var graph = rankedWins[currentTier];
                rankedOffsets[currentTier] += accountTeam.Won ? 1 : -1;

                var rank = _mapper.Map<ValorantRank>(currentTier);
                graph.Label = $"{rank.Tier} {rank.Ranking}";
                graph.ColorHex = ValorantRank.RankedColorMap[ValorantRank.RankMap[currentTier / 3].ToLower()];
                graph?.Data?.Add(new CoordinatePair() { X = currentMatch.MatchInfo.GameStartMillis, Y = rankedOffsets[currentTier] });
                previousTier = currentTier;
            }

            var graphData = rankedWins.Values.ToList();

            return new LineGraph
            {
                Data = graphData.ToList(),
                Title = "Ranked Wins"
            };
        }

        public async Task<PieChart> GetRecentlyUsedOperatorsPieChartAsync(Account account)
        {
            var matchHistory = await _riotClient.GetValorantGameHistory(account, 0, 15);
            if (matchHistory?.Any() is not true)
                return new PieChart();

            var matches = matchHistory.GroupBy((match) => _mapper.Map<ValorantCharacter>(match.Players.First((player) => player.Subject == account.PlatformId).CharacterId).Name);
            var pieChart = new PieChart();
            pieChart.Data = matches.Select((match) =>
            {
                return new PieChartData()
                {
                    Value = match.Count()
                };
            }).ToList();

            pieChart.Labels = matches.Select((match) => match.Key).ToList();
            pieChart.Title = "Recently Used Agents";
            return pieChart;
        }

        public async Task<LineGraph> GetRankedRRChangeLineGraph(Account account)
        {
            var matchHistory = await _riotClient.GetValorantCompetitiveHistory(account, 0, 15);
            if (matchHistory?.Matches?.Any() is not true)
                return new LineGraph();

            var matches = matchHistory.Matches.GroupBy((match) => match.TierAfterUpdate);
            var graphData = matches.Select((match) =>
            {
                var rank = _mapper.Map<ValorantRank>(match.Key);
                return new RankedGraphData()
                {
                    Data = match.Select((match) =>
                    {
                        return new CoordinatePair()
                        {
                            Y = match.RankedRatingAfterUpdate,
                            X = match.MatchStartTime
                        };
                    }).ToList(),
                    Tags = new(),
                    Label = $"{rank.Tier} {rank.Ranking} RR",
                    Hidden = match.Key != matchHistory.Matches.First().TierAfterUpdate,
                    ColorHex = ValorantRank.RankedColorMap[ValorantRank.RankMap[match.Key / 3].ToLower()]
                };
            }).OrderBy((graph) => graph.Hidden).ToList();

            return new LineGraph
            {
                Data = graphData,
                Title = "RR Change"
            };
        }
    }
}
