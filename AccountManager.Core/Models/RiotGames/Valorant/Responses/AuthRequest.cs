using System.Text.Json.Serialization;

namespace AccountManager.Core.Models.RiotGames.Valorant.Responses
{
    public class AuthRequest
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }
        [JsonPropertyName("username")]
        public string? Username { get; set; }
        [JsonPropertyName("password")]
        public string? Password { get; set; }
        [JsonPropertyName("remember")]
        public bool Remember { get; set; }
    }
}
