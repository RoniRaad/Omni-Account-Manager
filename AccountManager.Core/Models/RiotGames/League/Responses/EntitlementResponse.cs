using System.Text.Json.Serialization;

namespace AccountManager.Core.Models.RiotGames.League.Responses
{
    public class EntitlementResponse
    {
        [JsonPropertyName("entitlements_token")]
        public string? EntitlementToken { get; set; }
    }
}
