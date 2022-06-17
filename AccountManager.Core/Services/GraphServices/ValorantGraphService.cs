using AccountManager.Core.Enums;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Models.RiotGames.Valorant;
using AccountManager.Core.Models.RiotGames.Valorant.Responses;
using AccountManager.Core.Static;
using AutoMapper;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace AccountManager.Core.Services.GraphServices
{
    public class ValorantGraphService : IValorantGraphService
    {
        private readonly IRiotClient _riotClient;
        private readonly AlertService _alertService;
        private readonly IMemoryCache _memoryCache;
        private readonly IDistributedCache _persistantCache;
        private readonly IMapper _mapper;
        const AccountType accountType = AccountType.Valorant;
        const string cacheKeyFormat = "{0}.{1}.{2}";
        public ValorantGraphService(IRiotClient riotClient, AlertService alertService,
            IMemoryCache memoryCache, IMapper mapper, IDistributedCache persistantCache)
        {
            _riotClient = riotClient;
            _alertService = alertService;
            _memoryCache = memoryCache;
            _mapper = mapper;
            _persistantCache = persistantCache;
        }
        public async Task<LineGraph> GetRankedWinsLineGraph(Account account)
        {
            var cacheKey = string.Format(cacheKeyFormat, account.Username, nameof(GetRankedWinsLineGraph), accountType);
            LineGraph? lineGraph = await _persistantCache.GetAsync<LineGraph>(cacheKey);
            if (lineGraph is not null)
                return lineGraph;

            IEnumerable<ValorantMatch> matchHistory = new List<ValorantMatch>();

            try
            {
                matchHistory = await _riotClient.GetValorantGameHistory(account, 0, 15) ?? new List<ValorantMatch>();
            }
            catch
            {
                _alertService.AddErrorMessage("There was an issue getting your ranked wins graph. Try again later.");
            }

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

            lineGraph = new LineGraph
            {
                Data = graphData,
                Title = "Ranked Wins"
            };

            if (lineGraph is not null)
                await _persistantCache.SetAsync(cacheKey, lineGraph, new TimeSpan(0, 30, 0));

            return lineGraph;
        }

        public async Task<BarChart> GetRankedACS(Account account)
        {
            var cacheKey = string.Format(cacheKeyFormat, account.Username, nameof(GetRankedWinsLineGraph), accountType);
            BarChart? barChart = await _persistantCache.GetAsync<BarChart>(cacheKey);
            if (barChart is not null)
                return barChart;

            IEnumerable<ValorantMatch> matchHistory = new List<ValorantMatch>();

            try
            {
                matchHistory = await _riotClient.GetValorantGameHistory(account, 0, 15) ?? new List<ValorantMatch>();
            }
            catch
            {
                _alertService.AddErrorMessage("There was an issue getting your average acs chart. Try again later.");
            }

            if (matchHistory?.Any() is not true)
                return new BarChart();

            matchHistory = matchHistory.OrderBy(match => match.MatchInfo.GameStartMillis);
            var groupedMatches = matchHistory.GroupBy(match => _mapper.Map<ValorantCharacter>(match.Players.First((player) => player.Subject == account.PlatformId).CharacterId).Name);

            var barChartData = groupedMatches.Select(group =>
            {
                return new BarChartData()
                {
                    Value = group.Average(match =>
                    {

                        var accountInMatch = match?.Players?.First(player => player?.Subject == account?.PlatformId);

                        if (accountInMatch is null)
                            return 0;

                        return (accountInMatch?.Stats?.Score / accountInMatch?.Stats?.RoundsPlayed) ?? 0;
                    })
                };
            }).ToList();

            barChart = new BarChart
            {
                Labels = groupedMatches.Select(group => group.Key).ToList(),
                Data = barChartData,
                Title = "Average ACS"
            };

            if (barChart is not null)
                await _persistantCache.SetAsync(cacheKey, barChart, new TimeSpan(0, 30, 0));

            return barChart;
        }

        public async Task<PieChart> GetRecentlyUsedOperatorsPieChartAsync(Account account)
        {
            var cacheKey = string.Format(cacheKeyFormat, account.Username, nameof(GetRecentlyUsedOperatorsPieChartAsync), accountType);
            PieChart? pieChart = await _persistantCache.GetAsync<PieChart>(cacheKey);
            if (pieChart is not null)
                return pieChart;

            IEnumerable<ValorantMatch> matchHistory = new List<ValorantMatch>();

            try
            {
                matchHistory = await _riotClient.GetValorantGameHistory(account, 0, 15) ?? new List<ValorantMatch>();
            }
            catch
            {
                _alertService.AddErrorMessage("There was an issue getting your recently used operators chart. Try again later.");
            }

            if (matchHistory?.Any() is not true)
                return new PieChart();

            var matches = matchHistory.GroupBy((match) => _mapper.Map<ValorantCharacter>(match.Players.First((player) => player.Subject == account.PlatformId).CharacterId).Name);
            pieChart = new PieChart();
            var dataList = new List<PieChartData>();
            pieChart.Labels = new();

            foreach (var match in matches)
            {
                dataList.Add(new PieChartData()
                {
                    Value = match.Count()
                });

                pieChart.Labels.Add(match.Key);
            }

            pieChart.Data = dataList;
            pieChart.Title = "Recently Used Agents";

            if (pieChart is not null)
                await _persistantCache.SetAsync(cacheKey, pieChart, new TimeSpan(0, 30, 0));

            return pieChart;
        }

        public async Task<LineGraph> GetRankedRRChangeLineGraph(Account account)
        {
            var cacheKey = string.Format(cacheKeyFormat, account.Username, nameof(GetRankedRRChangeLineGraph), accountType);
            LineGraph? lineGraph = await _persistantCache.GetAsync<LineGraph>(cacheKey);
            if (lineGraph is not null)
                return lineGraph;

            ValorantRankedHistoryResponse matchHistory = new ValorantRankedHistoryResponse();

            try
            {
                matchHistory = await _riotClient.GetValorantCompetitiveHistory(account, 0, 15) ?? new ValorantRankedHistoryResponse();
            }
            catch
            {
                _alertService.AddErrorMessage("There was an issue getting your rr change graph. Try again later.");
            }

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

            lineGraph = new LineGraph
            {
                Data = graphData,
                Title = "RR Change"
            };

            if (lineGraph is not null)
                await _persistantCache.SetAsync(cacheKey, lineGraph, new TimeSpan(0, 30, 0));

            return lineGraph;
        }
    }
}
