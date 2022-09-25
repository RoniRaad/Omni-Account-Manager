using System.Text.Json.Serialization;

namespace AccountManager.Core.Models.RiotGames.Valorant.Responses
{
    public sealed class UserInfoResponse
    {
        [JsonPropertyName("sub")]
        public string? PuuId { get; set; }
    }
}