using System.Text.Json.Serialization;

namespace AccountManager.Core.Models.RiotGames.Valorant.Responses
{

    public sealed class History
    {
        [JsonPropertyName("MatchID")]
        public string MatchID { get; set; } = string.Empty;

        [JsonPropertyName("GameStartTime")]
        public object? GameStartTime { get; set; }

        [JsonPropertyName("QueueID")]
        public string QueueID { get; set; } = string.Empty;
    }

    public sealed class ValorantGameHistoryDataResponse
    {
        [JsonPropertyName("Subject")]
        public string Subject { get; set; } = string.Empty;

        [JsonPropertyName("BeginIndex")]
        public int BeginIndex { get; set; }

        [JsonPropertyName("EndIndex")]
        public int EndIndex { get; set; }

        [JsonPropertyName("Total")]
        public int Total { get; set; }

        [JsonPropertyName("History")]
        public List<History> History { get; set; } = new();
    }
}