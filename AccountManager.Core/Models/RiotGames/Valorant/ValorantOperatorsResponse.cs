namespace AccountManager.Core.Models.RiotGames.Valorant;
using System.Text.Json.Serialization;

public class Ability
{
    [JsonPropertyName("slot")]
    public string? Slot { get; set; }

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("displayIcon")]
    public string? DisplayIcon { get; set; }
}

public class Datum
{
    [JsonPropertyName("uuid")]
    public string? Uuid { get; set; }

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("developerName")]
    public string? DeveloperName { get; set; }

    [JsonPropertyName("characterTags")]
    public List<string>? CharacterTags { get; set; }

    [JsonPropertyName("displayIcon")]
    public string? DisplayIcon { get; set; }

    [JsonPropertyName("displayIconSmall")]
    public string? DisplayIconSmall { get; set; }

    [JsonPropertyName("bustPortrait")]
    public string? BustPortrait { get; set; }

    [JsonPropertyName("fullPortrait")]
    public string? FullPortrait { get; set; }

    [JsonPropertyName("fullPortraitV2")]
    public string? FullPortraitV2 { get; set; }

    [JsonPropertyName("killfeedPortrait")]
    public string? KillfeedPortrait { get; set; }

    [JsonPropertyName("background")]
    public string? Background { get; set; }

    [JsonPropertyName("backgroundGradientColors")]
    public List<string>? BackgroundGradientColors { get; set; }

    [JsonPropertyName("assetPath")]
    public string? AssetPath { get; set; }

    [JsonPropertyName("isFullPortraitRightFacing")]
    public bool? IsFullPortraitRightFacing { get; set; }

    [JsonPropertyName("isPlayableCharacter")]
    public bool? IsPlayableCharacter { get; set; }

    [JsonPropertyName("isAvailableForTest")]
    public bool? IsAvailableForTest { get; set; }

    [JsonPropertyName("isBaseContent")]
    public bool? IsBaseContent { get; set; }

    [JsonPropertyName("role")]
    public Role? Role { get; set; }

    [JsonPropertyName("abilities")]
    public List<Ability>? Abilities { get; set; }

    [JsonPropertyName("voiceLine")]
    public VoiceLine? VoiceLine { get; set; }
}

public class MediaList
{
    [JsonPropertyName("id")]
    public int? Id { get; set; }

    [JsonPropertyName("wwise")]
    public string? Wwise { get; set; }

    [JsonPropertyName("wave")]
    public string? Wave { get; set; }
}

public class Role
{
    [JsonPropertyName("uuid")]
    public string? Uuid { get; set; }

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("displayIcon")]
    public string? DisplayIcon { get; set; }

    [JsonPropertyName("assetPath")]
    public string? AssetPath { get; set; }
}

public class ValorantOperatorsResponse
{
    [JsonPropertyName("status")]
    public int? Status { get; set; }

    [JsonPropertyName("data")]
    public List<Datum>? Data { get; set; }
}

public class VoiceLine
{
    [JsonPropertyName("minDuration")]
    public double? MinDuration { get; set; }

    [JsonPropertyName("maxDuration")]
    public double? MaxDuration { get; set; }

    [JsonPropertyName("mediaList")]
    public List<MediaList>? MediaList { get; set; }
}

