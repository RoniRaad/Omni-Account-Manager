using System.Text.Json.Serialization;

namespace AccountManager.Core.Models.RiotGames.League.Requests
{
    public sealed class LoginRequest
    {
        [JsonPropertyName("clientName")]
        public string Name { get; set; } = "lcu";
        [JsonPropertyName("entitlements")]
        public string? Entitlements { get; set; }
        [JsonPropertyName("userinfo")]
        public string? UserInfo { get; set; }
    }
}
