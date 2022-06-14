using AccountManager.Core.Enums;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Models.RiotGames.League;
using AccountManager.Core.Static;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace AccountManager.Core.Services.GraphServices
{
    public class LeagueGraphService : ILeagueGraphService
    {
        private readonly ILeagueClient _leagueClient;
        private readonly IRiotClient _riotClient;
        private readonly IMemoryCache _memoryCache;
        private readonly IDistributedCache _persistantCache;
        const AccountType accountType = AccountType.League;
        const string cacheKeyFormat = "{0}.{1}.{2}";

        public LeagueGraphService(ILeagueClient leagueClient, IRiotClient riotClient, 
            IMemoryCache memoryCache, IDistributedCache persistantCache)
        {
            _leagueClient = leagueClient;
            _riotClient = riotClient;
            _memoryCache = memoryCache;
            _persistantCache = persistantCache;
        }

        public async Task<LineGraph> GetRankedWinsGraph(Account account)
        {
            var cacheKey = string.Format(cacheKeyFormat, account.Username, nameof(GetRankedWinsGraph), accountType);
            LineGraph? lineGraph = await _persistantCache.GetAsync<LineGraph>(cacheKey);
            if (lineGraph is not null)
                return lineGraph;

            lineGraph = new();

            try
            {
                var soloQueueRank = await _leagueClient.GetSummonerRankByPuuidAsync(account);

                if (string.IsNullOrEmpty(account.PlatformId))
                    account.PlatformId = await _riotClient.GetPuuId(account.Username, account.Password);
                if (string.IsNullOrEmpty(account.PlatformId))
                    return new();

                var matchHistoryResponse = await _leagueClient.GetUserLeagueMatchHistory(account, 0, 15);
                var queueMapping = await _leagueClient.GetLeagueQueueMappings();

                var gameMatchesByType = new Dictionary<string, RankedGraphData>();
                var gameMatchesOffsetByType = new Dictionary<string, int>();

                for (int i = matchHistoryResponse?.Games?.Count - 1 ?? 0; i >= 0; i--)
                {
                    var game = matchHistoryResponse?.Games[i];
                    var queueName = queueMapping?.FirstOrDefault(queue => queue.QueueId == game?.Json?.QueueId);

                    if (game is not null && 
                        game?.Json?.GameCreation is not null && 
                        queueName?.Description?.Contains("Teamfights Tactics") is false)
                    {
                        var usersTeam = game?.Json?.Participants?.FirstOrDefault((participant) => participant?.Puuid == account?.PlatformId, null)?.TeamId;
                        
                        var gameMatch = new GameMatch()
                        {
                            Id = game?.Json?.GameId?.ToString() ?? "None",
                            GraphValueChange = game?.Json?.Teams?.FirstOrDefault((team) => team?.TeamId == usersTeam, null)?.Win ?? false ? 1 : -1,
                            EndTime = DateTimeOffset.FromUnixTimeMilliseconds(game?.Json?.GameCreation ?? 0).ToLocalTime(),
                            Type = queueMapping?.FirstOrDefault((map) => map?.QueueId == game?.Json?.QueueId, null)?.Description
                                    ?.Replace("games", "")
                                    ?.Replace("5v5", "")
                                    ?.Replace("Ranked", "")
                                    ?.Trim() ?? "Other"
                        };

                        if (!gameMatchesOffsetByType.TryGetValue(gameMatch.Type, out var offset))
                        {
                            gameMatchesOffsetByType[gameMatch.Type] = 0;
                            gameMatch.GraphValueChange = 0;
                        }

                        if (!gameMatchesByType.TryGetValue(gameMatch.Type, out var gameList))
                        {
                            gameList = new();
                            gameList.Label = gameMatch.Type;
                            gameMatchesByType.Add(gameMatch.Type, gameList);
                            if (gameMatch.Type == "Solo")
                                gameList.ColorHex = soloQueueRank.HexColor;
                        }

                        gameMatchesOffsetByType[gameMatch.Type] += gameMatch.GraphValueChange;
                        gameMatchesByType[gameMatch.Type].Data.Add(new CoordinatePair() { Y = gameMatchesOffsetByType[gameMatch.Type], X = DateTimeOffset.FromUnixTimeMilliseconds(game?.Json?.GameCreation ?? 0).ToLocalTime().ToUnixTimeMilliseconds() });
                    }
                }

                lineGraph.Title = "Ranked Wins";
                lineGraph.Data = gameMatchesByType.Values.OrderBy((dataset) => !string.IsNullOrEmpty(dataset.ColorHex) ? 1 : 0).ToList();

                if (lineGraph is not null)
                    await _persistantCache.SetAsync(cacheKey, lineGraph, new TimeSpan(0, 30, 0));

                return lineGraph;
            }
            catch
            {
                return new();
            }
        }

        public async Task<PieChart> GetRankedChampSelectPieChart(Account account)
        {
            var cacheKey = string.Format(cacheKeyFormat, account.Username, nameof(GetRankedChampSelectPieChart), accountType);
            PieChart? pieChart = await _persistantCache.GetAsync<PieChart>(cacheKey);

            if (pieChart is not null)
                return pieChart;

            var matchHistoryResponse = new UserChampSelectHistory();
            try
            {
                if (string.IsNullOrEmpty(account.PlatformId))
                    account.PlatformId = await _riotClient.GetPuuId(account.Username, account.Password);
                if (string.IsNullOrEmpty(account.PlatformId))
                    return new();

                matchHistoryResponse = await _leagueClient.GetUserChampSelectHistory(account, 0, 40);
                pieChart = new();

                if (matchHistoryResponse is null)
                    return new();

                matchHistoryResponse.Champs = matchHistoryResponse.Champs.OrderByDescending((champ) => champ.SelectedCount);

                pieChart.Data = matchHistoryResponse.Champs.Select((champs) =>
                {
                    return new PieChartData()
                    {
                        Value = champs.SelectedCount
                    };
                });
                pieChart.Title = "Recently Used Champs";
                pieChart.Labels = matchHistoryResponse.Champs.Select((champ) => champ.ChampName).ToList();
                
                if (pieChart is not null)
                    await _persistantCache.SetAsync(cacheKey, pieChart, new TimeSpan(0, 30, 0));

                return pieChart;
            }
            catch
            {
                return new();
            }
        }


        public async Task<BarChart> GetRankedWinrateByChampBarChartAsync(Account account)
        {
            var cacheKey = string.Format(cacheKeyFormat, account.Username, nameof(GetRankedWinrateByChampBarChartAsync), accountType);
            BarChart? barChart = await _persistantCache.GetAsync<BarChart>(cacheKey);

            if (barChart is not null)
                return barChart;

            try
            {
                if (string.IsNullOrEmpty(account.PlatformId))
                    account.PlatformId = await _riotClient.GetPuuId(account.Username, account.Password);
                if (string.IsNullOrEmpty(account.PlatformId))
                    return new();

                var matchHistoryResponse = await _leagueClient.GetUserLeagueMatchHistory(account, 0, 40);
                barChart = new();
                if (matchHistoryResponse is null)
                    return new();

                var matchesGroupedByChamp = matchHistoryResponse?.Games?.GroupBy((game) => game?.Json?.Participants?.FirstOrDefault((participant) => participant.Puuid == account.PlatformId, null)?.ChampionName);

                barChart.Labels = matchesGroupedByChamp?.Select((matchGrouping) => matchGrouping.Key)?.Where((matchGrouping) => matchGrouping is not null).ToList();

                var barChartData = matchesGroupedByChamp?.Select((matchGrouping) =>
                {
                    return new BarChartData
                    {
                        Value = (double)matchGrouping.Sum((match) => match?.Json?.Participants?.FirstOrDefault((participant) => participant?.Puuid == account?.PlatformId, null)?.Win is true ? 1 : 0) / (double)matchGrouping.Count()
                    };
                });

                barChart.Data = barChartData;
                barChart.Title = "Recent Winrate";
                barChart.Type = "percent";

                if (barChart is not null)
                    await _persistantCache.SetAsync(cacheKey, barChart, new TimeSpan(0, 30, 0));

                return barChart;
            }
            catch
            {
                return new();
            }
        }


        public async Task<BarChart> GetRankedCsRateByChampBarChartAsync(Account account)
        {
            var cacheKey = string.Format(cacheKeyFormat, account.Username, nameof(GetRankedCsRateByChampBarChartAsync), accountType);
            BarChart? barChart = await _persistantCache.GetAsync<BarChart>(cacheKey);

            if (barChart is not null)
                return barChart;

            try
            {
                if (string.IsNullOrEmpty(account.PlatformId))
                    account.PlatformId = await _riotClient.GetPuuId(account.Username, account.Password);
                if (string.IsNullOrEmpty(account.PlatformId))
                    return new();

                var matchHistoryResponse = await _leagueClient.GetUserLeagueMatchHistory(account, 0, 40);
                if (matchHistoryResponse is null)
                    return new();

                matchHistoryResponse.Games = matchHistoryResponse?.Games?.Where((game) => game?.Json?.GameDuration > 15 * 60).ToList();

                barChart = new();
                var matchesGroupedByChamp = matchHistoryResponse?.Games?.GroupBy((game) => game?.Json?.Participants?.FirstOrDefault((participant) => participant.Puuid == account.PlatformId, null)?.ChampionName);
                barChart.Labels = matchesGroupedByChamp?.Select((matchGrouping) => matchGrouping.Key)?.Where((matchGrouping) => matchGrouping is not null).ToList();

                var barChartData = matchesGroupedByChamp?.Select((matchGrouping) =>
                {
                    var average = matchGrouping.Average((match) =>
                    {
                        var player = match?.Json?.Participants?.FirstOrDefault((participant) => participant?.Puuid == account?.PlatformId, null);
                        var minionsKilled = player?.TotalMinionsKilled + player?.NeutralMinionsKilled;
                        var matchMinutes = match?.Json?.GameDuration / 60;
                        var minionsKilledPerMinute = minionsKilled / matchMinutes;
                        if (minionsKilled is null || matchMinutes is null)
                            return 0;

                        return minionsKilledPerMinute;
                    });

                    return new BarChartData
                    {
                        Value = average
                    };
                });

                barChart.Data = barChartData;
                barChart.Title = "Recent CS Per Minute";

                if (barChart is not null)
                    await _persistantCache.SetAsync(cacheKey, barChart, new TimeSpan(0, 30, 0));

                return barChart;
            }
            catch
            {
                return new();
            }
        }
    }
}
