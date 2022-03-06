using System.Text.Json.Serialization;

namespace AccountManager.Core.Models.League
{
    public class LeagueSummonerRank
    {
        [JsonPropertyName("queueMap")]
        public QueueMap QueueMap { get; set; }
    }
    public class RankedSoloDuo
    {
        [JsonPropertyName("tier")]
        public string Tier { get; set; }
        [JsonPropertyName("division")]
        public string Division { get; set; }
    }

    public class QueueMap
    {
        [JsonPropertyName("RANKED_SOLO_5x5")]
        public RankedSoloDuo RankedSoloDuoStats { get; set; }
    }
}
