using System.Text.Json.Serialization;

namespace AccountManager.Core.Models
{
    public class ExchangeCodeResponse
    {
        [JsonPropertyName("code")]
        public string ExchangeCode { get; set; } = "";
    }
}