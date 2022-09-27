using System.Text.Json.Serialization;

namespace AccountManager.Core.Models.RiotGames.League
{
    public sealed class SplitsProgress
    {
        [JsonPropertyName("1")]
        public int One { get; set; }
    }
}
