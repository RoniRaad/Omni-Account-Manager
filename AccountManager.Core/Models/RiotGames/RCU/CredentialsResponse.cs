using System.Text.Json.Serialization;

namespace AccountManager.Infrastructure.Services.Platform
{
    public partial class RiotClientApi
    {
        public class CredentialsResponse
        {
            [JsonPropertyName("authenticationType")]
            public string? AuthenticationType { get; set; }

            [JsonPropertyName("country")]
            public string? Country { get; set; }

            [JsonPropertyName("error")]
            public string? Error { get; set; }

            [JsonPropertyName("multifactor")]
            public MultifactorResponse? Multifactor { get; set; }

            [JsonPropertyName("persistLogin")]
            public bool? PersistLogin { get; set; }

            [JsonPropertyName("securityProfile")]
            public string? SecurityProfile { get; set; }

            [JsonPropertyName("type")]
            public string? Type { get; set; }
        }
    }
}
