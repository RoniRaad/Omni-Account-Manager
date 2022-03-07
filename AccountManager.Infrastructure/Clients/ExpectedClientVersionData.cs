using System.Text.Json.Serialization;

namespace AccountManager.Infrastructure.Clients
{
    public partial class RiotClient
    {
        public class ExpectedClientVersionData
        {
            [JsonPropertyName("riotClientVersion")]
            public string? RiotClientVersion { get; set; }

        }
    }
}
