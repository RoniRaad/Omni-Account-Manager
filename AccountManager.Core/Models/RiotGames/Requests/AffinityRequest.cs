using System.Text.Json.Serialization;

namespace AccountManager.Infrastructure.Clients
{
    public class AffinityRequest
    {
        [JsonPropertyName("id_token")]
        public string IdToken { get; set; }
    }
}
