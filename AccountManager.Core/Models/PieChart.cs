using System.Text.Json.Serialization;

namespace AccountManager.Core.Models
{
    public class PieChart
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }
        [JsonPropertyName("labels")]
        public List<string>? Labels { get; set; }
        public IEnumerable<PieChartData> Data { get; set; }
    }
}