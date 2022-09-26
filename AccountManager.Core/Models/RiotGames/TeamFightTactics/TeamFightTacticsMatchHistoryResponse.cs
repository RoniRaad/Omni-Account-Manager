using System.Text.Json.Serialization;
namespace AccountManager.Core.Models.RiotGames.TeamFightTactics.Responses;

public sealed class TeamFightTacticsMatchHistory
{
    [JsonPropertyName("active_puuid")]
    public string? ActivePuuid { get; set; }

    [JsonPropertyName("games")]
    public List<Game>? Games { get; set; }

    public sealed class Companion
    {
        [JsonPropertyName("content_ID")]
        public string? ContentID { get; set; }

        [JsonPropertyName("skin_ID")]
        public int SkinID { get; set; }

        [JsonPropertyName("species")]
        public string? Species { get; set; }
    }

    public sealed class Game
    {
        [JsonPropertyName("json")]
        public Json? Json { get; set; }

        [JsonPropertyName("metadata")]
        public Metadata? Metadata { get; set; }
    }

    public sealed class Json
    {
        [JsonPropertyName("game_datetime")]
        public string? GameDatetime { get; set; }

        [JsonPropertyName("game_id")]
        public object? GameId { get; set; }

        [JsonPropertyName("game_length")]
        public double GameLength { get; set; }

        [JsonPropertyName("game_version")]
        public string? GameVersion { get; set; }

        [JsonPropertyName("participants")]
        public List<Participant>? Participants { get; set; }

        [JsonPropertyName("queue_id")]
        public int QueueId { get; set; }

        [JsonPropertyName("tft_game_type")]
        public string? TftGameType { get; set; }

        [JsonPropertyName("tft_set_number")]
        public int TftSetNumber { get; set; }
    }

    public sealed class Metadata
    {
        [JsonPropertyName("data_version")]
        public int DataVersion { get; set; }

        [JsonPropertyName("info_type")]
        public string? InfoType { get; set; }

        [JsonPropertyName("match_id")]
        public string? MatchId { get; set; }

        [JsonPropertyName("participants")]
        public List<string>? Participants { get; set; }

        [JsonPropertyName("private")]
        public bool Private { get; set; }

        [JsonPropertyName("product")]
        public string? Product { get; set; }

        [JsonPropertyName("tags")]
        public List<string>? Tags { get; set; }

        [JsonPropertyName("timestamp")]
        public long? Timestamp { get; set; }
    }

    public sealed class Participant
    {
        [JsonPropertyName("augments")]
        public List<string>? Augments { get; set; }

        [JsonPropertyName("companion")]
        public Companion? Companion { get; set; }

        [JsonPropertyName("gold_left")]
        public int GoldLeft { get; set; }

        [JsonPropertyName("last_round")]
        public int LastRound { get; set; }

        [JsonPropertyName("level")]
        public int Level { get; set; }

        [JsonPropertyName("placement")]
        public int Placement { get; set; }

        [JsonPropertyName("players_eliminated")]
        public int PlayersEliminated { get; set; }

        [JsonPropertyName("puuid")]
        public string? Puuid { get; set; }

        [JsonPropertyName("time_eliminated")]
        public double TimeEliminated { get; set; }

        [JsonPropertyName("total_damage_to_players")]
        public int TotalDamageToPlayers { get; set; }

        [JsonPropertyName("traits")]
        public List<Trait>? Traits { get; set; }

        [JsonPropertyName("units")]
        public List<Unit>? Units { get; set; }
    }


    public sealed class Trait
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("num_units")]
        public int NumUnits { get; set; }

        [JsonPropertyName("style")]
        public int Style { get; set; }

        [JsonPropertyName("tier_current")]
        public int TierCurrent { get; set; }

        [JsonPropertyName("tier_total")]
        public int TierTotal { get; set; }
    }

    public sealed class Unit
    {
        [JsonPropertyName("character_id")]
        public string? CharacterId { get; set; }

        [JsonPropertyName("items")]
        public List<int>? Items { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("rarity")]
        public int Rarity { get; set; }

        [JsonPropertyName("tier")]
        public int Tier { get; set; }
    }
}
