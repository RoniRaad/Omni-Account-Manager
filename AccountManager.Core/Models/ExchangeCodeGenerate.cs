using System.Text.Json.Serialization;

namespace AccountManager.Core.Models
{
    public sealed class ExchangeCodeResponse
    {
        [JsonPropertyName("code")]
        public string ExchangeCode { get; set; } = "";
    }
}