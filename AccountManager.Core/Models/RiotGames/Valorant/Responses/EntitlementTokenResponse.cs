using System.Text.Json.Serialization;

namespace AccountManager.Core.Models.RiotGames.Valorant.Responses
{
    public sealed class EntitlementTokenResponse
    {
        [JsonPropertyName("entitlements_token")]
        public string? EntitlementToken { get; set; }
    }
}
