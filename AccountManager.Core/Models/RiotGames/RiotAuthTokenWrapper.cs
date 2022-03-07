using AccountManager.Core.Models.RiotGames.League.Responses;
using System.Text.Json.Serialization;

namespace AccountManager.Core.Models.RiotGames
{
    public class RiotAuthTokenWrapper
    {
        [JsonPropertyName("response")]
        public RiotAuthTokenResponse Response { get; set; }
    }
}
