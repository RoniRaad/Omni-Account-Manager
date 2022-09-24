using AccountManager.Core.Static;
using System.Text.Json.Serialization;

namespace AccountManager.Core.Models.RiotGames.Requests
{
    public sealed class RiotTokenRequest
    {
        [JsonPropertyName("client_id")]
        public string? Id { get; set; }
        public string? Nonce { get; set; }
        [JsonPropertyName("redirect_uri")]
        public string? RedirectUri { get; set; }
        [JsonPropertyName("response_type")]
        public string? ResponseType { get; set; }
        [JsonPropertyName("scope")]
        public string? Scope { get; set; }
        public string GetHashId()
        {
            return StringEncryption.Hash($"{Id}.{Nonce}.{RedirectUri}.{ResponseType}.{Scope}");
        }
    }
}
