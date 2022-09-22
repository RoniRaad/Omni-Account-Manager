using System.Text.Json.Serialization;

namespace AccountManager.Core.Models.RiotGames.League
{
    public sealed class Claims
    {
        [JsonPropertyName("cname")]
        public string? CName { get; set; }
    }
}
