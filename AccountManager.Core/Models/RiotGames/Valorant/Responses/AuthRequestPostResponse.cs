using System.Text.Json.Serialization;

namespace AccountManager.Core.Models.RiotGames.Valorant.Responses
{
    public sealed class AuthRequestPostResponse
    {
        [JsonPropertyName("client_id")]
        public string? Id { get; set; }
        [JsonPropertyName("nonce")]
        public string? Nonce { get; set; }
        [JsonPropertyName("redirect_uri")]
        public string? RedirectUri { get; set; }
        [JsonPropertyName("response_type")]
        public string? ResponseType { get; set; }
    }
}
