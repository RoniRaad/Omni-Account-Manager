using System.Text.Json.Serialization;

namespace AccountManager.Core.Models.RiotGames.League.Responses
{
    public sealed class LeagueRankedResponse
    {
        [JsonPropertyName("queues")]
        public List<Queue>? Queues { get; set; }

        [JsonPropertyName("highestPreviousSeasonEndTier")]
        public string? HighestPreviousSeasonEndTier { get; set; }

        [JsonPropertyName("highestPreviousSeasonEndRank")]
        public string? HighestPreviousSeasonEndRank { get; set; }

        [JsonPropertyName("earnedRegaliaRewardIds")]
        public List<object>? EarnedRegaliaRewardIds { get; set; }

        [JsonPropertyName("splitsProgress")]
        public SplitsProgress? SplitsProgress { get; set; }

        [JsonPropertyName("seasons")]
        public Seasons? Seasons { get; set; }
    }
}
