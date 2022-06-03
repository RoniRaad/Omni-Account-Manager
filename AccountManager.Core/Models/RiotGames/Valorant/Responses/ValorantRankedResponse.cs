using System.Text.Json.Serialization;

namespace AccountManager.Core.Models.RiotGames.Valorant.Responses
{
    public class Match
    {
        [JsonPropertyName("MatchID")]
        public string? MatchID { get; set; }

        [JsonPropertyName("MapID")]
        public string? MapID { get; set; }

        [JsonPropertyName("SeasonID")]
        public string? SeasonID { get; set; }

        [JsonPropertyName("MatchStartTime")]
        public double MatchStartTime { get; set; }

        [JsonPropertyName("TierAfterUpdate")]
        public int TierAfterUpdate { get; set; }

        [JsonPropertyName("TierBeforeUpdate")]
        public int TierBeforeUpdate { get; set; }

        [JsonPropertyName("RankedRatingAfterUpdate")]
        public int RankedRatingAfterUpdate { get; set; }

        [JsonPropertyName("RankedRatingBeforeUpdate")]
        public int RankedRatingBeforeUpdate { get; set; }

        [JsonPropertyName("RankedRatingEarned")]
        public int RankedRatingEarned { get; set; }

        [JsonPropertyName("RankedRatingPerformanceBonus")]
        public int RankedRatingPerformanceBonus { get; set; }

        [JsonPropertyName("CompetitiveMovement")]
        public string? CompetitiveMovement { get; set; }

        [JsonPropertyName("AFKPenalty")]
        public int AFKPenalty { get; set; }
    }

    public class ValorantRankedResponse
    {
        [JsonPropertyName("Version")]
        public int Version { get; set; }

        [JsonPropertyName("Subject")]
        public string? Subject { get; set; }

        [JsonPropertyName("Matches")]
        public List<Match>? Matches { get; set; }
    }


}