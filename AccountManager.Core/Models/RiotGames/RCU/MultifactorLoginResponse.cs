using System.Text.Json.Serialization;

namespace AccountManager.Infrastructure.Services.Platform
{
    public partial class RiotClientApi
    {
        public class MultifactorLoginResponse
        {
            [JsonPropertyName("code")]
            public string? Code { get; set; }
            [JsonPropertyName("retry")]
            public bool? Retry { get; set; }
            [JsonPropertyName("trustDevice")]
            public bool? TrustDevice { get; set; }
        }
    }
}
