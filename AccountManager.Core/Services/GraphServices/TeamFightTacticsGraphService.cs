
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using Microsoft.Extensions.Caching.Memory;

namespace AccountManager.Core.Services.GraphServices
{
    public class TeamFightTacticsGraphService : ITeamFightTacticsGraphService
    {
        private readonly ILeagueClient _leagueClient;
        private readonly IRiotClient _riotClient;
        private readonly IMemoryCache _memoryCache;

        public TeamFightTacticsGraphService(ILeagueClient leagueClient, IRiotClient riotClient,
            IMemoryCache memoryCache)
        {
            _leagueClient = leagueClient;
            _riotClient = riotClient;
            _memoryCache = memoryCache;
        }
        public async Task<LineGraph> GetRankedPlacementOffset(Account account)
        {
            var rankCacheString = $"{account.Username}.tft.rankgraphdata";
            if (_memoryCache.TryGetValue(rankCacheString, out LineGraph? rankedGraphDataSets) && rankedGraphDataSets is not null)
                return rankedGraphDataSets;

            try
            {
                if (string.IsNullOrEmpty(account.PlatformId))
                    account.PlatformId = await _riotClient.GetPuuId(account.Username, account.Password);
                if (string.IsNullOrEmpty(account.PlatformId))
                    return new();

                var matchHistoryResponse = await _leagueClient.GetUserTeamFightTacticsMatchHistory(account, 0, 10);

                var queueMapping = await _leagueClient.GetLeagueQueueMappings();

                if (matchHistoryResponse is null && queueMapping is null)
                    return null;

                var matchHistory = new UserMatchHistory()
                {
                    Matches = matchHistoryResponse?.Games
                    ?.Select((game) =>
                    {
                        if (game is not null && game?.Metadata?.Timestamp is not null)
                            return new GameMatch()
                            {
                                Id = game?.Json?.GameId?.ToString() ?? "None",
                                // 4th place grants no value while going up and down adds 1 positive and negative value for each movement
                                GraphValueChange = (game?.Json?.Participants?.First((participant) => participant.Puuid == account.PlatformId)?.Placement - 4) * -1 ?? 0,
                                EndTime = DateTimeOffset.FromUnixTimeMilliseconds(game?.Metadata?.Timestamp ?? 0).ToLocalTime(),
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
                var soloQueueRank = await _leagueClient.GetTFTRankByPuuidAsync(account);

                var matchesGroups = matchHistory?.Matches?.Reverse()?.GroupBy((match) => match.Type);
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
                    if (rankedGraphData.Label == "Teamfight Tactics")
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

                if (matchHistoryResponse is null)
                    return new();

                rankedGraphDataSets.Data = rankedGraphDataSets.Data.OrderByDescending((dataset) => string.IsNullOrEmpty(dataset.ColorHex)).ToList();
                rankedGraphDataSets.Title = "Ranked Placement Offset";

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
