using System.Text.Json.Serialization;

namespace AccountManager.Core.Models
{
    public class BarChart
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }
        [JsonPropertyName("labels")]
        public List<string>? Labels { get; set; }
        public IEnumerable<BarChartData>? Data { get; set; }
        public string Type { get; set; } = string.Empty;
    }
}