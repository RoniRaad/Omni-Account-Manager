using AccountManager.Core.Static;
using System.Text.Json.Serialization;

namespace AccountManager.Core.Models.RiotGames.Requests
{
    public sealed class RiotTokenRequest
    {
        [JsonPropertyName("client_id")]
        public string? Id { get; set; }
        [JsonPropertyName("acr_values")]
        public string? Acr { get; set; } = "";
        [JsonPropertyName("claims")]
        public string? Claims { get; set; } = "{\r\n    \"id_token\": {\r\n        \"rgn_NA1\": null\r\n    },\r\n    \"userinfo\": {\r\n        \"rgn_NA1\": null\r\n    }\r\n}";
        [JsonPropertyName("code_challenge")]
        public string? CodeChallenge { get; set; } = "";
        [JsonPropertyName("code_challenge_method")]
        public string? CodeChallengeMethod { get; set; } = "";
        [JsonPropertyName("nonce")]
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
