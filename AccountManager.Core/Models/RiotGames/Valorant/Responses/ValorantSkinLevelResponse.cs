using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AccountManager.Core.Models.RiotGames.Valorant.Responses
{
    public class Data
    {
        [JsonPropertyName("uuid")]
        public string Uuid { get; set; }

        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }

        [JsonPropertyName("levelItem")]
        public object LevelItem { get; set; }

        [JsonPropertyName("displayIcon")]
        public string DisplayIcon { get; set; }

        [JsonPropertyName("streamedVideo")]
        public string StreamedVideo { get; set; }

        [JsonPropertyName("assetPath")]
        public string AssetPath { get; set; }
        public int Price { get; set; }
    }

    public class ValorantSkinLevelResponse
    {
        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("data")]
        public Data Data { get; set; }
    }
}
