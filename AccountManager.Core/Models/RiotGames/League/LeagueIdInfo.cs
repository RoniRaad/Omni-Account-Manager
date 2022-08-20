using System.Text.Json.Serialization;

namespace AccountManager.Core.Models.RiotGames.League
{
    public class LeagueIdInfo
    {
        [JsonPropertyName("uid")]
        public long Uid { get; set; }

        [JsonPropertyName("cuid")]
        public long Cuid { get; set; }

        [JsonPropertyName("uname")]
        public string? Uname { get; set; }

        [JsonPropertyName("cpid")]
        public string? Cpid { get; set; }

        [JsonPropertyName("ptrid")]
        public object? Ptrid { get; set; }

        [JsonPropertyName("pid")]
        public string? Pid { get; set; }

        [JsonPropertyName("state")]
        public string? State { get; set; }
    }
}
