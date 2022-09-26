using System.Text.Json.Serialization;

namespace AccountManager.Infrastructure.Services.Platform
{
    public partial class RiotClientApi
    {
        public sealed class MultifactorResponse
        {
            [JsonPropertyName("email")]
            public string? Email { get; set; }

            [JsonPropertyName("method")]
            public string? Method { get; set; }

            [JsonPropertyName("methods")]
            public List<string>? Methods { get; set; }

            [JsonPropertyName("mfaVersion")]
            public string? MfaVersion { get; set; }

            [JsonPropertyName("multiFactorCodeLength")]
            public int? MultiFactorCodeLength { get; set; }
        }
    }
}
