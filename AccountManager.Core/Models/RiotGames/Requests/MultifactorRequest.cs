using System.Text.Json.Serialization;

namespace AccountManager.Core.Models.RiotGames.Requests
{
    public class MultifactorRequest
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("rememberDevice")]
        public bool RememberDevice { get; set; }
    }
}
