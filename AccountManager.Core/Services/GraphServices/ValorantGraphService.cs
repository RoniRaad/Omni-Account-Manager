using AccountManager.Core.Enums;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Models.RiotGames.Valorant;
using AccountManager.Core.Models.RiotGames.Valorant.Responses;
using AccountManager.Core.Static;
using AccountManager.Infrastructure.Clients;
using AutoMapper;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace AccountManager.Core.Services.GraphServices
{
    public class ValorantGraphService : IValorantGraphService
    {
        private readonly AlertService _alertService;
        private readonly IValorantClient _valorantClient;
        private readonly IMapper _mapper;
        public ValorantGraphService(AlertService alertService, IMapper mapper,
             IDistributedCache persistantCache,
             IValorantClient valorantClient)
        {
            _alertService = alertService;
            _mapper = mapper;
            _valorantClient = valorantClient;
        }
        public async Task<LineGraph> GetRankedWinsLineGraph(Account account)
        {
            LineGraph? lineGraph = new();

            IEnumerable<ValorantMatch> matchHistory = new List<ValorantMatch>();

            try
            {
                matchHistory = await _valorantClient.GetValorantGameHistory(account) ?? new List<ValorantMatch>();
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

            return lineGraph ?? new();
        }

        public async Task<BarChart> GetRankedACS(Account account)
        {
            IEnumerable<ValorantMatch> matchHistory = new List<ValorantMatch>();

            try
            {
                matchHistory = await _valorantClient.GetValorantGameHistory(account) ?? new List<ValorantMatch>();
            }
            catch
            {
                _alertService.AddErrorMessage("There was an issue getting your average acs chart. Try again later.");
            }

            if (matchHistory?.Any() is not true)
                return new BarChart();

            var operators = await _valorantClient.GetValorantOperators();

            matchHistory = matchHistory.OrderBy(match => match.MatchInfo.GameStartMillis);
            var groupedMatches = matchHistory.GroupBy(match => operators?.Data?.First((op) => op.Uuid == match.Players.First((player) => player.Subject == account.PlatformId).CharacterId));

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

            var barChart = new BarChart
            {
                Labels = groupedMatches?.Select(group => group?.Key?.DisplayName ?? "UNKNOWN CHARACTER")?.ToList() ?? new(),
                Data = barChartData,
                Title = "Average ACS"
            };

            return barChart ?? new();
        }

        public async Task<PieChart> GetRecentlyUsedOperatorsPieChartAsync(Account account)
        {
            PieChart? pieChart;

            IEnumerable<ValorantMatch> matchHistory = new List<ValorantMatch>();

            try
            {
                matchHistory = await _valorantClient.GetValorantGameHistory(account) ?? new List<ValorantMatch>();
            }
            catch
            {
                _alertService.AddErrorMessage("There was an issue getting your recently used operators chart. Try again later.");
            }

            if (matchHistory?.Any() is not true)
                return new PieChart();

            var operators = await _valorantClient.GetValorantOperators();
            var matches = matchHistory.GroupBy((match) => operators?.Data?.First((op) => match.Players.First((player) => player.Subject == account.PlatformId).CharacterId == op.Uuid));
            pieChart = new PieChart();
            var dataList = new List<PieChartData>();
            pieChart.Labels = new();

            foreach (var match in matches)
            {
                dataList.Add(new PieChartData()
                {
                    Value = match.Count()
                });

                pieChart.Labels.Add(match?.Key?.DisplayName ?? "UNKNOWN OPERATOR");
            }

            pieChart.Data = dataList;
            pieChart.Title = "Recently Used Agents";

            return pieChart ?? new();
        }

        public async Task<LineGraph> GetRankedRRChangeLineGraph(Account account)
        {
            LineGraph? lineGraph = new();

            ValorantRankedHistoryResponse matchHistory = new ValorantRankedHistoryResponse();

            try
            {
                matchHistory = await _valorantClient.GetValorantCompetitiveHistory(account) ?? new ValorantRankedHistoryResponse();
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

            return lineGraph ?? new();
        }
    }
}
