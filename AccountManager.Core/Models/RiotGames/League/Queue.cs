using System.Text.Json.Serialization;

namespace AccountManager.Core.Models.RiotGames.League
{
    public class Queue
    {
        [JsonPropertyName("queueType")]
        public string QueueType { get; set; }

        [JsonPropertyName("provisionalGameThreshold")]
        public int ProvisionalGameThreshold { get; set; }

        [JsonPropertyName("tier")]
        public string Tier { get; set; }

        [JsonPropertyName("rank")]
        public string Rank { get; set; }

        [JsonPropertyName("leaguePoints")]
        public int LeaguePoints { get; set; }

        [JsonPropertyName("wins")]
        public int Wins { get; set; }

        [JsonPropertyName("losses")]
        public int Losses { get; set; }

        [JsonPropertyName("provisionalGamesRemaining")]
        public int ProvisionalGamesRemaining { get; set; }

        [JsonPropertyName("previousSeasonEndTier")]
        public string PreviousSeasonEndTier { get; set; }

        [JsonPropertyName("previousSeasonEndRank")]
        public string PreviousSeasonEndRank { get; set; }

        [JsonPropertyName("ratedRating")]
        public int RatedRating { get; set; }
    }
}
