using System.Text.Json.Serialization;

namespace AccountManager.Core.Models.EpicGames
{
    public sealed class ExchangeCodeResponse
    {
        [JsonPropertyName("code")]
        public string ExchangeCode { get; set; } = "";
    }
}