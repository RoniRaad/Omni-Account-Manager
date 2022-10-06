using System.Text.Json.Serialization;

namespace AccountManager.Infrastructure.Services.Platform
{
    public partial class RiotClientApi
    {
        public sealed class LoginRequest
        {
            [JsonPropertyName("password")]
            public string? Password { get; set; }
            [JsonPropertyName("persistLogin")]
            public bool? PersistLogin { get; set; }
            [JsonPropertyName("username")]
            public string? Username { get; set; }
        }
    }
}
