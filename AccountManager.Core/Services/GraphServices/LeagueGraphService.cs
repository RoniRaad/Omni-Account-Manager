using AccountManager.Core.Enums;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Models.RiotGames.League;

namespace AccountManager.Core.Services.GraphServices
{
    public class LeagueGraphService : ILeagueGraphService
    {
        private readonly ILeagueClient _leagueClient;
        private readonly IRiotClient _riotClient;

        public LeagueGraphService(ILeagueClient leagueClient, IRiotClient riotClient)
        {
            _leagueClient = leagueClient;
            _riotClient = riotClient;
        }

        public async Task<LineGraph?> GetRankedWinsGraph(Account account)
        {
            LineGraph lineGraph;

            try
            {
                var soloQueueRank = await _leagueClient.GetSummonerRankByPuuidAsync(account);

                if (string.IsNullOrEmpty(account.PlatformId))
                    account.PlatformId = await _riotClient.GetPuuId(account);
                if (string.IsNullOrEmpty(account.PlatformId))
                    return new();

                var matchHistoryResponse = await _leagueClient.GetUserLeagueMatchHistory(account);
                var queueMapping = await _leagueClient.GetLeagueQueueMappings();

                var gameMatchesByType = new Dictionary<string, RankedGraphData>();
                var gameMatchesOffsetByType = new Dictionary<string, int>();

                for (int i = matchHistoryResponse?.Games?.Count - 1 ?? 0; i >= 0; i--)
                {
                    var game = matchHistoryResponse?.Games?.ElementAt(i) ?? new();
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
                            gameList = new()
                            {
                                Label = gameMatch.Type
                            };
                            gameMatchesByType.Add(gameMatch.Type, gameList);
                            if (gameMatch.Type == "Solo")
                                gameList.ColorHex = soloQueueRank.HexColor;
                        }

                        gameMatchesOffsetByType[gameMatch.Type] += gameMatch.GraphValueChange;
                        gameMatchesByType[gameMatch.Type].Data.Add(new CoordinatePair() { Y = gameMatchesOffsetByType[gameMatch.Type], X = DateTimeOffset.FromUnixTimeMilliseconds(game?.Json?.GameCreation ?? 0).ToLocalTime().ToUnixTimeMilliseconds() });
                    }
                }
                lineGraph = new()
                {
                    Title = "Ranked Wins",
                    Data = gameMatchesByType.Values.OrderBy((dataset) => !string.IsNullOrEmpty(dataset.ColorHex) ? 1 : 0).ToList()
                };


                return lineGraph;
            }
            catch
            {
                return null;
            }
        }

        public async Task<PieChart?> GetRankedChampSelectPieChart(Account account)
        {
            PieChart? pieChart;

            var matchHistoryResponse = new UserChampSelectHistory();
            try
            {
                if (string.IsNullOrEmpty(account.PlatformId))
                    account.PlatformId = await _riotClient.GetPuuId(account);
                if (string.IsNullOrEmpty(account.PlatformId))
                    return new();

                matchHistoryResponse = await _leagueClient.GetUserChampSelectHistory(account);
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
                
                return pieChart;
            }
            catch
            {
                return null;
            }
        }


        public async Task<BarChart?> GetRankedWinrateByChampBarChartAsync(Account account)
        {
            BarChart? barChart;

            try
            {
                if (string.IsNullOrEmpty(account.PlatformId))
                    account.PlatformId = await _riotClient.GetPuuId(account);
                if (string.IsNullOrEmpty(account.PlatformId))
                    return new();

                var matchHistoryResponse = await _leagueClient.GetUserLeagueMatchHistory(account);
                barChart = new();
                if (matchHistoryResponse is null)
                    return new();

                var matchesGroupedByChamp = matchHistoryResponse?.Games?.GroupBy((game) => game?.Json?.Participants?.FirstOrDefault((participant) => participant?.Puuid == account.PlatformId, null)?.ChampionName);

                barChart.Labels = matchesGroupedByChamp?.Select((matchGrouping) => matchGrouping?.Key ?? "Unknown")?.Where((matchGrouping) => matchGrouping is not null)?.ToList() ?? new();

                var barChartData = matchesGroupedByChamp?.Select((matchGrouping) =>
                {
                    return new BarChartData
                    {
                        Value = (double)matchGrouping.Sum((match) => match?.Json?.Participants?.FirstOrDefault((participant) => participant?.Puuid == account?.PlatformId, null)?.Win is true ? 1 : 0) / (double)matchGrouping.Count()
                    };
                });

                barChart.Data = barChartData ?? new List<BarChartData>();
                barChart.Title = "Recent Winrate";
                barChart.Type = "percent";

                return barChart ?? new();
            }
            catch
            {
                return null;
            }
        }


        public async Task<BarChart?> GetRankedCsRateByChampBarChartAsync(Account account)
        {
            BarChart? barChart = new();

            try
            {
                if (string.IsNullOrEmpty(account.PlatformId))
                    account.PlatformId = await _riotClient.GetPuuId(account);
                if (string.IsNullOrEmpty(account.PlatformId))
                    return new();

                var matchHistoryResponse = await _leagueClient.GetUserLeagueMatchHistory(account);
                if (matchHistoryResponse is null)
                    return new();

                matchHistoryResponse.Games = matchHistoryResponse?.Games?.Where((game) => game?.Json?.GameDuration > 15 * 60).ToList();

                barChart = new();
                var matchesGroupedByChamp = matchHistoryResponse?.Games?.GroupBy((game) => game?.Json?.Participants?.FirstOrDefault((participant) => participant?.Puuid == account.PlatformId, null)?.ChampionName);
                barChart.Labels = matchesGroupedByChamp?.Select((matchGrouping) => matchGrouping?.Key ?? "Unknown")?.Where((matchGrouping) => matchGrouping is not null).ToList();

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

                barChart.Data = barChartData ?? new List<BarChartData>();
                barChart.Title = "Recent CS Per Minute";

                return barChart ?? new();
            }
            catch
            {
                return null;
            }
        }
    }
}
