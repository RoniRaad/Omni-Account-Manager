using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Models.RiotGames.League;
using Microsoft.Extensions.Caching.Memory;

namespace AccountManager.Core.Services.GraphServices
{
    public class LeagueGraphService : ILeagueGraphService
    {
        private readonly ILeagueClient _leagueClient;
        private readonly IRiotClient _riotClient;
        private readonly HttpClient _httpClient;
        private readonly AlertService _alertService;
        private readonly IMemoryCache _memoryCache;

        public LeagueGraphService(ILeagueClient leagueClient, IRiotClient riotClient, IMemoryCache memoryCache)
        {
            _leagueClient = leagueClient;
            _riotClient = riotClient;
            _memoryCache = memoryCache;
        }

        public async Task<LineGraph> GetRankedWinsGraph(Account account)
        {
            var rankCacheString = $"{account.Username}.leagueoflegends.{nameof(GetRankedWinsGraph)}";
            if (_memoryCache.TryGetValue(rankCacheString, out LineGraph? rankedGraphDataSets) && rankedGraphDataSets is not null)
                return rankedGraphDataSets;

            try
            {
                if (string.IsNullOrEmpty(account.PlatformId))
                    account.PlatformId = await _riotClient.GetPuuId(account.Username, account.Password);
                if (string.IsNullOrEmpty(account.PlatformId))
                    return new();

                var matchHistoryResponse = await _leagueClient.GetUserLeagueMatchHistory(account, 0, 15);
                var queueMapping = await _leagueClient.GetLeagueQueueMappings();

                var userMatchHistory = new UserMatchHistory()
                {
                    Matches = matchHistoryResponse?.Games
                    ?.Where((game) => queueMapping?.FirstOrDefault((map) => map?.QueueId == game?.Json?.QueueId, null)?.Description?.Contains("Teamfights Tactics") is false)
                    ?.Select((game) =>
                    {
                        var usersTeam = game?.Json?.Participants?.FirstOrDefault((participant) => participant?.Puuid == account?.PlatformId, null)?.TeamId;

                        if (game is not null && game?.Json?.GameCreation is not null)
                            return new GameMatch()
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

                        return new();
                    })
                };

                rankedGraphDataSets = new();
                var soloQueueRank = await _leagueClient.GetSummonerRankByPuuidAsync(account);

                var matchesGroups = userMatchHistory?.Matches?.Reverse()?.GroupBy((match) => match.Type);
                if (matchesGroups is null)
                    return new();

                var orderedGroups = matchesGroups.Select((group) => group.OrderBy((match) => match.EndTime));

                foreach (var matchGroup in orderedGroups)
                {
                    var matchWinDelta = 0;
                    var isFirst = true;
                    var rankedGraphData = new RankedGraphData()
                    {
                        Label = matchGroup.FirstOrDefault(new GameMatch())?.Type ?? "Other",
                        Data = new(),
                        Tags = new()
                    };
                    if (rankedGraphData.Label == "Solo")
                        rankedGraphData.ColorHex = soloQueueRank.HexColor;

                    foreach (var match in matchGroup)
                    {
                        if (!isFirst)
                        {
                            matchWinDelta += match.GraphValueChange;
                        }

                        var dateTime = match.EndTime;

                        rankedGraphData.Data.Add(new CoordinatePair() { Y = matchWinDelta, X = dateTime.ToUnixTimeMilliseconds() });
                        isFirst = false;
                    }


                    if (rankedGraphData.Data.Count > 1)
                        rankedGraphDataSets.Data.Add(rankedGraphData);
                }

                if (userMatchHistory is not null)
                    _memoryCache.Set(rankCacheString, rankedGraphDataSets, TimeSpan.FromHours(1));

                if (userMatchHistory is null)
                    return new();

                rankedGraphDataSets.Data = rankedGraphDataSets.Data.OrderBy((dataset) => !string.IsNullOrEmpty(dataset.ColorHex) ? 1 : 0).ToList();
                rankedGraphDataSets.Title = "Ranked Wins";

                return rankedGraphDataSets;
            }
            catch
            {
                return new();
            }
        }

        public async Task<PieChart> GetRankedChampSelectPieChart(Account account)
        {
            var rankCacheString = $"{account.Username}.leagueoflegends.{nameof(GetRankedChampSelectPieChart)}";
            if (_memoryCache.TryGetValue(rankCacheString, out PieChart? rankedGraphDataSets) && rankedGraphDataSets is not null)
                return rankedGraphDataSets;

            var matchHistoryResponse = new UserChampSelectHistory();
            try
            {
                if (string.IsNullOrEmpty(account.PlatformId))
                    account.PlatformId = await _riotClient.GetPuuId(account.Username, account.Password);
                if (string.IsNullOrEmpty(account.PlatformId))
                    return new();

                matchHistoryResponse = await _leagueClient.GetUserChampSelectHistory(account, 0, 40);
                rankedGraphDataSets = new();

                if (matchHistoryResponse is null)
                    return new();

                matchHistoryResponse.Champs = matchHistoryResponse.Champs.OrderByDescending((champ) => champ.SelectedCount);

                rankedGraphDataSets.Data = matchHistoryResponse.Champs.Select((champs) =>
                {
                    return new PieChartData()
                    {
                        Value = champs.SelectedCount
                    };
                });
                rankedGraphDataSets.Title = "Recently Used Champs";
                rankedGraphDataSets.Labels = matchHistoryResponse.Champs.Select((champ) => champ.ChampName).ToList();
                
                if (rankedGraphDataSets is not null)
                    _memoryCache.Set(rankCacheString, rankedGraphDataSets, TimeSpan.FromHours(1));

                return rankedGraphDataSets;
            }
            catch
            {
                return new();
            }
        }


        public async Task<BarChart> GetRankedWinrateByChampBarChartAsync(Account account)
        {
            var rankCacheString = $"{account.Username}.leagueoflegends.{nameof(GetRankedWinrateByChampBarChartAsync)}";
            if (_memoryCache.TryGetValue(rankCacheString, out BarChart? rankedGraphDataSets) && rankedGraphDataSets is not null)
                return rankedGraphDataSets;

            try
            {
                if (string.IsNullOrEmpty(account.PlatformId))
                    account.PlatformId = await _riotClient.GetPuuId(account.Username, account.Password);
                if (string.IsNullOrEmpty(account.PlatformId))
                    return new();

                var matchHistoryResponse = await _leagueClient.GetUserLeagueMatchHistory(account, 0, 40);
                rankedGraphDataSets = new();
                if (matchHistoryResponse is null)
                    return new();

                var matchesGroupedByChamp = matchHistoryResponse?.Games?.GroupBy((game) => game?.Json?.Participants?.FirstOrDefault((participant) => participant.Puuid == account.PlatformId, null)?.ChampionName);

                rankedGraphDataSets.Labels = matchesGroupedByChamp?.Select((matchGrouping) => matchGrouping.Key)?.Where((matchGrouping) => matchGrouping is not null).ToList();

                var barChartData = matchesGroupedByChamp?.Select((matchGrouping) =>
                {
                    return new BarChartData
                    {
                        Value = (double)matchGrouping.Sum((match) => match?.Json?.Participants?.FirstOrDefault((participant) => participant?.Puuid == account?.PlatformId, null)?.Win is true ? 1 : 0) / (double)matchGrouping.Count()
                    };
                });

                rankedGraphDataSets.Data = barChartData;
                rankedGraphDataSets.Title = "Recent Winrate";
                rankedGraphDataSets.Type = "percent";

                if (rankedGraphDataSets is not null)
                    _memoryCache.Set(rankCacheString, rankedGraphDataSets, TimeSpan.FromHours(1));

                return rankedGraphDataSets;
            }
            catch
            {
                return new();
            }
        }


        public async Task<BarChart> GetRankedCsRateByChampBarChartAsync(Account account)
        {
            var rankCacheString = $"{account.Username}.leagueoflegends.{nameof(GetRankedCsRateByChampBarChartAsync)}";
            if (_memoryCache.TryGetValue(rankCacheString, out BarChart? rankedGraphDataSets) && rankedGraphDataSets is not null)
                return rankedGraphDataSets;

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

                rankedGraphDataSets = new();
                var matchesGroupedByChamp = matchHistoryResponse?.Games?.GroupBy((game) => game?.Json?.Participants?.FirstOrDefault((participant) => participant.Puuid == account.PlatformId, null)?.ChampionName);
                rankedGraphDataSets.Labels = matchesGroupedByChamp?.Select((matchGrouping) => matchGrouping.Key)?.Where((matchGrouping) => matchGrouping is not null).ToList();

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

                rankedGraphDataSets.Data = barChartData;
                rankedGraphDataSets.Title = "Recent CS Per Minute";
                if (rankedGraphDataSets is not null)
                    _memoryCache.Set(rankCacheString, rankedGraphDataSets, TimeSpan.FromHours(1));

                return rankedGraphDataSets;
            }
            catch
            {
                return new();
            }
        }
    }
}
