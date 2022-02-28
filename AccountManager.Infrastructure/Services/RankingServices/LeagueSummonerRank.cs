using System.Text.Json.Serialization;

namespace AccountManager.Infrastructure.Services.RankingServices
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
