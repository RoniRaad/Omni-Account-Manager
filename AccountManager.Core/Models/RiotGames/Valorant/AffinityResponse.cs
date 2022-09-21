using System.Text.Json.Serialization;

namespace AccountManager.Core.Models.RiotGames.Valorant
{
    public class AffinityResponse
    {
        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;

        [JsonPropertyName("affinities")]
        public Affinities? Afinity { get; set; }

        public class Affinities
        {
            [JsonPropertyName("pbe")]
            public string Pbe { get; set; } = string.Empty;

            [JsonPropertyName("live")]
            public string Live { get; set; } = string.Empty;
        }
    }
}
