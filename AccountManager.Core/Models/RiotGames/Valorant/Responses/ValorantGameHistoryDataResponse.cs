using System.Text.Json.Serialization;

namespace AccountManager.Core.Models.RiotGames.Valorant.Responses
{

    public class History
    {
        [JsonPropertyName("MatchID")]
        public string MatchID { get; set; }

        [JsonPropertyName("GameStartTime")]
        public object GameStartTime { get; set; }

        [JsonPropertyName("QueueID")]
        public string QueueID { get; set; }
    }

    public class ValorantGameHistoryDataResponse
    {
        [JsonPropertyName("Subject")]
        public string Subject { get; set; }

        [JsonPropertyName("BeginIndex")]
        public int BeginIndex { get; set; }

        [JsonPropertyName("EndIndex")]
        public int EndIndex { get; set; }

        [JsonPropertyName("Total")]
        public int Total { get; set; }

        [JsonPropertyName("History")]
        public List<History> History { get; set; }
    }
}