using System.Text.Json.Serialization;

namespace AccountManager.UI.Services
{
    public class ExchangeCodeGenerate
    {
        [JsonPropertyName("code")]
        public string ExchangeCode { get; set; } = "";
    }
}