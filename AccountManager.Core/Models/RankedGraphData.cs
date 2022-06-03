namespace AccountManager.Core.Models
{
    public class RankedGraphData
    {
        public string? Label { get; set; }
        public List<string[]>? Tags { get; set; }
        public List<CoordinatePair>? Data { get; set; }
        public string? ColorHex { get; set; }
        public bool Hidden { get; set; } = false;
    }
}
