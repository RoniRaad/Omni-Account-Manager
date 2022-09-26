using AccountManager.Core.Models.RiotGames.League.Responses;
using System.Text.Json.Serialization;

namespace AccountManager.Core.Models.RiotGames
{
    public sealed class Multifactor
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

    public sealed class RiotAuthTokenWrapper
    {
        [JsonPropertyName("response")]
        public RiotAuthTokenResponse? Response { get; set; }
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("multifactor")]
        public Multifactor? Multifactor { get; set; }

        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("securityProfile")]
        public string? SecurityProfile { get; set; }
    }
}
