using System.Text.Json.Serialization;

namespace AccountManager.Core.Models.RiotGames.League.Responses
{
    public sealed class LeagueQueueMapResponse
    {
        [JsonPropertyName("queueId")]
        public int QueueId { get; set; }

        [JsonPropertyName("map")]
        public string? Map { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("notes")]
        public string? Notes { get; set; }
    }
}
