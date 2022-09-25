using System.Text.Json.Serialization;

namespace AccountManager.Core.Models.RiotGames.Valorant
{
    public sealed class ValorantQueueSkills
    {
        [JsonPropertyName("competitive")]
        public ValorantCompetitive? Competitive { get; set; }
    }
}