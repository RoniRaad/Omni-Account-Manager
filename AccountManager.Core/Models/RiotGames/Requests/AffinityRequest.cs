using System.Text.Json.Serialization;

namespace AccountManager.Infrastructure.Clients
{
    public sealed class AffinityRequest
    {
        [JsonPropertyName("id_token")]
        public string IdToken { get; set; } = string.Empty;
    }
}
