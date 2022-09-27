using System.Text.Json.Serialization;

namespace AccountManager.Infrastructure.Services.Platform
{
    public partial class RiotClientApi
    {
        public sealed class AuthFlowStartRequest
        {
            [JsonPropertyName("loginStrategy")]
            public string? LoginStrategy { get; set; }

            [JsonPropertyName("persistLogin")]
            public bool? PersistLogin { get; set; }

            [JsonPropertyName("requireRiotID")]
            public bool? RequireRiotID { get; set; }

            [JsonPropertyName("scopes")]
            public List<string>? Scopes { get; set; }
        }
    }
}
