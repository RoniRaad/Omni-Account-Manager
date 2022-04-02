using System.Text.Json.Serialization;

namespace AccountManager.Core.Models.RiotGames.Valorant
{
    public class Multifactor
    {
        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("method")]
        public string? Method { get; set; }

        [JsonPropertyName("methods")]
        public List<string>? Methods { get; set; }

        [JsonPropertyName("multiFactorCodeLength")]
        public int MultiFactorCodeLength { get; set; }

        [JsonPropertyName("mfaVersion")]
        public string? MfaVersion { get; set; }
    }
}