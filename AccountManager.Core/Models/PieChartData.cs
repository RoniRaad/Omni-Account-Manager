using System.Text.Json.Serialization;

namespace AccountManager.Core.Models
{
    public class PieChartData
    {
        [JsonPropertyName("value")]
        public int Value { get; set; }
    }
}