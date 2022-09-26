using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AccountManager.Core.Models.RiotGames.Valorant.Responses
{
    public sealed class Data
    {
        [JsonPropertyName("uuid")]
        public string Uuid { get; set; } = string.Empty;

        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; } = string.Empty;

        [JsonPropertyName("levelItem")]
        public object? LevelItem { get; set; }

        [JsonPropertyName("displayIcon")]
        public string DisplayIcon { get; set; } = string.Empty;

        [JsonPropertyName("streamedVideo")]
        public string StreamedVideo { get; set; } = string.Empty;

        [JsonPropertyName("assetPath")]
        public string AssetPath { get; set; } = string.Empty;
        public int Price { get; set; } = 0;
    }

    public sealed class ValorantSkinLevelResponse
    {
        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("data")]
        public Data Data { get; set; } = new Data();
    }
}
