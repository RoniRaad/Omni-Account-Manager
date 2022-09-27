namespace AccountManager.Core.Models
{
    public sealed class RankedGraphData
    {
        public string? Label { get; set; }
        public List<string[]>? Tags { get; set; }
        public List<CoordinatePair> Data { get; set; } = new List<CoordinatePair>();
        public string? ColorHex { get; set; }
        public bool Hidden { get; set; } = false;
    }
}
