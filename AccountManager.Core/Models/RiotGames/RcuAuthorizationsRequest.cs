using System.Text.Json.Serialization;

namespace AccountManager.Core.Models.RiotGames
{
    public class RcuAuthorizationsRequest
    {
        [JsonPropertyName("clientId")]
        public string ClientId { get; set; } = "riot-client";

        [JsonPropertyName("trustLevels")]
        public List<string> TrustLevels { get; set; } = new() { "always_trusted" };
    }
}
