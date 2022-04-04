using System.Text.Json.Serialization;

namespace AccountManager.Core.Models.RiotGames.Valorant.Responses
{
    public class UserInfoResponse
    {
        [JsonPropertyName("sub")]
        public string? PuuId { get; set; }
    }
}