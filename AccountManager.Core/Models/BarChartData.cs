using System.Text.Json.Serialization;

namespace AccountManager.Core.Models
{
    public sealed class BarChartData
    {
        [JsonPropertyName("value")]
        public double? Value { get; set; }
    }
}