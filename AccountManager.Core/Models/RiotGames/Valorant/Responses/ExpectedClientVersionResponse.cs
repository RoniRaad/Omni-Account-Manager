using System.Text.Json.Serialization;
namespace AccountManager.Core.Models.RiotGames.Valorant.Responses
{
    public sealed class ExpectedClientVersionResponse
    {

        [JsonPropertyName("data")]
        public ExpectedClientVersionData? Data { get; set; }
    }
}
