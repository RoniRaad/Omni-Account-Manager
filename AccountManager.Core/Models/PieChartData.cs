using System.Text.Json.Serialization;

namespace AccountManager.Core.Models
{
    public sealed class PieChartData
    {
        [JsonPropertyName("value")]
        public int Value { get; set; }
    }
}