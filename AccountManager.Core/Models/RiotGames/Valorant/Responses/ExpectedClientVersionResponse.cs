using System.Text.Json.Serialization;
namespace AccountManager.Core.Models.RiotGames.Valorant.Responses
{
    public class ExpectedClientVersionResponse
    {

        [JsonPropertyName("data")]
        public ExpectedClientVersionData? Data { get; set; }
    }
}
