using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AccountManager.Core.Models.RiotGames.Valorant.Responses
{
    public sealed class Ability
    {
        [JsonPropertyName("idleTimeMillis")]
        public double IdleTimeMillis { get; set; }

        [JsonPropertyName("objectiveCompleteTimeMillis")]
        public double ObjectiveCompleteTimeMillis { get; set; }

        [JsonPropertyName("grenadeEffects")]
        public object? GrenadeEffects { get; set; }

        [JsonPropertyName("ability1Effects")]
        public object? Ability1Effects { get; set; }

        [JsonPropertyName("ability2Effects")]
        public object? Ability2Effects { get; set; }

        [JsonPropertyName("ultimateEffects")]
        public object? UltimateEffects { get; set; }
    }

    public sealed class AbilityCasts
    {
        [JsonPropertyName("grenadeCasts")]
        public double GrenadeCasts { get; set; }

        [JsonPropertyName("ability1Casts")]
        public double Ability1Casts { get; set; }

        [JsonPropertyName("ability2Casts")]
        public double Ability2Casts { get; set; }

        [JsonPropertyName("ultimateCasts")]
        public double UltimateCasts { get; set; }
    }

    public sealed class AdaptiveBots
    {
        [JsonPropertyName("idleTimeMillis")]
        public double IdleTimeMillis { get; set; }

        [JsonPropertyName("objectiveCompleteTimeMillis")]
        public double ObjectiveCompleteTimeMillis { get; set; }

        [JsonPropertyName("adaptiveBotAverageDurationMillisAllAttempts")]
        public double AdaptiveBotAverageDurationMillisAllAttempts { get; set; }

        [JsonPropertyName("adaptiveBotAverageDurationMillisFirstAttempt")]
        public double AdaptiveBotAverageDurationMillisFirstAttempt { get; set; }

        [JsonPropertyName("killDetailsFirstAttempt")]
        public object? KillDetailsFirstAttempt { get; set; }
    }

    public sealed class BasicGunSkill
    {
        [JsonPropertyName("idleTimeMillis")]
        public double IdleTimeMillis { get; set; }

        [JsonPropertyName("objectiveCompleteTimeMillis")]
        public double ObjectiveCompleteTimeMillis { get; set; }
    }

    public sealed class BasicMovement
    {
        [JsonPropertyName("idleTimeMillis")]
        public double IdleTimeMillis { get; set; }

        [JsonPropertyName("objectiveCompleteTimeMillis")]
        public double ObjectiveCompleteTimeMillis { get; set; }
    }

    public sealed class BehaviorFactors
    {
        [JsonPropertyName("afkRounds")]
        public double AfkRounds { get; set; }

        [JsonPropertyName("collisions")]
        public double Collisions { get; set; }

        [JsonPropertyName("damageParticipationOutgoing")]
        public double DamageParticipationOutgoing { get; set; }

        [JsonPropertyName("friendlyFireIncoming")]
        public double FriendlyFireIncoming { get; set; }

        [JsonPropertyName("friendlyFireOutgoing")]
        public double FriendlyFireOutgoing { get; set; }

        [JsonPropertyName("stayedInSpawnRounds")]
        public double StayedInSpawnRounds { get; set; }
    }

    public sealed class BombPlant
    {
        [JsonPropertyName("idleTimeMillis")]
        public double IdleTimeMillis { get; set; }

        [JsonPropertyName("objectiveCompleteTimeMillis")]
        public double ObjectiveCompleteTimeMillis { get; set; }
    }

    public sealed class Damage
    {
        [JsonPropertyName("receiver")]
        public string Receiver { get; set; } = string.Empty;

        [JsonPropertyName("damage")]
        public double DamageAmount { get; set; }

        [JsonPropertyName("legshots")]
        public double Legshots { get; set; }

        [JsonPropertyName("bodyshots")]
        public double Bodyshots { get; set; }

        [JsonPropertyName("headshots")]
        public double Headshots { get; set; }
    }

    public sealed class DefendBombSite
    {
        [JsonPropertyName("idleTimeMillis")]
        public double IdleTimeMillis { get; set; }

        [JsonPropertyName("objectiveCompleteTimeMillis")]
        public double ObjectiveCompleteTimeMillis { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }
    }

    public sealed class DefuseLocation
    {
        [JsonPropertyName("x")]
        public double X { get; set; }

        [JsonPropertyName("y")]
        public double Y { get; set; }
    }

    public sealed class DefusePlayerLocation
    {
        [JsonPropertyName("subject")]
        public string Subject { get; set; } = string.Empty;

        [JsonPropertyName("viewRadians")]
        public double ViewRadians { get; set; }

        [JsonPropertyName("location")]
        public Location Location { get; set; } = new();
    }

    public sealed class Economy
    {
        [JsonPropertyName("loadoutValue")]
        public double LoadoutValue { get; set; }

        [JsonPropertyName("weapon")]
        public string Weapon { get; set; } = string.Empty;

        [JsonPropertyName("armor")]
        public string Armor { get; set; } = string.Empty;

        [JsonPropertyName("remaining")]
        public double Remaining { get; set; }

        [JsonPropertyName("spent")]
        public double Spent { get; set; }
    }

    public sealed class FinishingDamage
    {
        [JsonPropertyName("damageType")]
        public string DamageType { get; set; } = string.Empty;

        [JsonPropertyName("damageItem")]
        public string DamageItem { get; set; } = string.Empty;

        [JsonPropertyName("isSecondaryFireMode")]
        public bool IsSecondaryFireMode { get; set; }
    }

    public sealed class Kill
    {
        [JsonPropertyName("gameTime")]
        public double GameTime { get; set; }

        [JsonPropertyName("roundTime")]
        public double RoundTime { get; set; }

        [JsonPropertyName("killer")]
        public string Killer { get; set; } = string.Empty;

        [JsonPropertyName("victim")]
        public string Victim { get; set; } = string.Empty;

        [JsonPropertyName("victimLocation")]
        public VictimLocation VictimLocation { get; set; } = new();

        [JsonPropertyName("assistants")]
        public List<string> Assistants { get; set; } = new();

        [JsonPropertyName("playerLocations")]
        public List<PlayerLocation> PlayerLocations { get; set; } = new();

        [JsonPropertyName("finishingDamage")]
        public FinishingDamage FinishingDamage { get; set; } = new();

        [JsonPropertyName("round")]
        public double Round { get; set; }
    }

    public sealed class Location
    {
        [JsonPropertyName("x")]
        public double X { get; set; }

        [JsonPropertyName("y")]
        public double Y { get; set; }
    }

    public sealed class MatchInfo
    {
        [JsonPropertyName("matchId")]
        public string MatchId { get; set; } = string.Empty;

        [JsonPropertyName("mapId")]
        public string MapId { get; set; } = string.Empty;

        [JsonPropertyName("gamePodId")]
        public string GamePodId { get; set; } = string.Empty;

        [JsonPropertyName("gameLoopZone")]
        public string GameLoopZone { get; set; } = string.Empty;

        [JsonPropertyName("gameServerAddress")]
        public string GameServerAddress { get; set; } = string.Empty;

        [JsonPropertyName("gameVersion")]
        public string GameVersion { get; set; } = string.Empty;

        [JsonPropertyName("gameLengthMillis")]
        public double GameLengthMillis { get; set; }

        [JsonPropertyName("gameStartMillis")]
        public long GameStartMillis { get; set; }

        [JsonPropertyName("provisioningFlowID")]
        public string ProvisioningFlowID { get; set; } = string.Empty;

        [JsonPropertyName("isCompleted")]
        public bool IsCompleted { get; set; }

        [JsonPropertyName("customGameName")]
        public string CustomGameName { get; set; } = string.Empty;

        [JsonPropertyName("forcePostProcessing")]
        public bool ForcePostProcessing { get; set; }

        [JsonPropertyName("queueID")]
        public string QueueID { get; set; } = string.Empty;

        [JsonPropertyName("gameMode")]
        public string GameMode { get; set; } = string.Empty;

        [JsonPropertyName("isRanked")]
        public bool IsRanked { get; set; }

        [JsonPropertyName("isMatchSampled")]
        public bool IsMatchSampled { get; set; }

        [JsonPropertyName("seasonId")]
        public string SeasonId { get; set; } = string.Empty;

        [JsonPropertyName("completionState")]
        public string CompletionState { get; set; } = string.Empty;

        [JsonPropertyName("platformType")]
        public string PlatformType { get; set; } = string.Empty;

        [JsonPropertyName("partyRRPenalties")]
        public PartyRRPenalties PartyRRPenalties { get; set; } = new();

        [JsonPropertyName("shouldMatchDisablePenalties")]
        public bool ShouldMatchDisablePenalties { get; set; }
    }

    public sealed class NewPlayerExperienceDetails
    {
        [JsonPropertyName("basicMovement")]
        public BasicMovement BasicMovement { get; set; } = new();

        [JsonPropertyName("basicGunSkill")]
        public BasicGunSkill BasicGunSkill { get; set; } = new();

        [JsonPropertyName("adaptiveBots")]
        public AdaptiveBots AdaptiveBots { get; set; } = new();

        [JsonPropertyName("ability")]
        public Ability Ability { get; set; } = new();

        [JsonPropertyName("bombPlant")]
        public BombPlant BombPlant { get; set; } = new();

        [JsonPropertyName("defendBombSite")]
        public DefendBombSite DefendBombSite { get; set; } = new();

        [JsonPropertyName("settingStatus")]
        public SettingStatus SettingStatus { get; set; } = new();
    }

    public sealed class PartyRRPenalties
    {
        [JsonPropertyName("12a93e78-9158-4500-8e5e-2fcfc6da9e59")]
        public double _12a93e78915845008e5e2fcfc6da9e59 { get; set; }

        [JsonPropertyName("3d7b88c3-1eb8-4ee4-908c-187b315a76e7")]
        public double _3d7b88c31eb84ee4908c187b315a76e7 { get; set; }

        [JsonPropertyName("4e1d27b7-6c7d-400d-ba61-e435689094dc")]
        public double _4e1d27b76c7d400dBa61E435689094dc { get; set; }

        [JsonPropertyName("82e0ff93-a69f-4464-ab54-28f87f4d0d09")]
        public double _82e0ff93A69f4464Ab5428f87f4d0d09 { get; set; }

        [JsonPropertyName("87d22a2d-d50b-4a86-94d7-8fc59eff3dff")]
        public double _87d22a2dD50b4a8694d78fc59eff3dff { get; set; }

        [JsonPropertyName("e4bb60a8-7283-44c6-a17f-0bc21b0a0230")]
        public double E4bb60a8728344c6A17f0bc21b0a0230 { get; set; }
    }

    public sealed class PlantLocation
    {
        [JsonPropertyName("x")]
        public double X { get; set; }

        [JsonPropertyName("y")]
        public double Y { get; set; }
    }

    public sealed class PlantPlayerLocation
    {
        [JsonPropertyName("subject")]
        public string Subject { get; set; } = string.Empty;

        [JsonPropertyName("viewRadians")]
        public double ViewRadians { get; set; }

        [JsonPropertyName("location")]
        public Location Location { get; set; } = new();
    }

    public sealed class PlatformInfo
    {
        [JsonPropertyName("platformType")]
        public string PlatformType { get; set; } = string.Empty;

        [JsonPropertyName("platformOS")]
        public string PlatformOS { get; set; } = string.Empty;

        [JsonPropertyName("platformOSVersion")]
        public string PlatformOSVersion { get; set; } = string.Empty;

        [JsonPropertyName("platformChipset")]
        public string PlatformChipset { get; set; } = string.Empty;
    }

    public sealed class Player
    {
        [JsonPropertyName("subject")]
        public string Subject { get; set; } = string.Empty;

        [JsonPropertyName("gameName")]
        public string GameName { get; set; } = string.Empty;

        [JsonPropertyName("tagLine")]
        public string TagLine { get; set; } = string.Empty;

        [JsonPropertyName("platformInfo")]
        public PlatformInfo PlatformInfo { get; set; } = new PlatformInfo();

        [JsonPropertyName("teamId")]
        public string TeamId { get; set; } = string.Empty;

        [JsonPropertyName("partyId")]
        public string PartyId { get; set; } = string.Empty;

        [JsonPropertyName("characterId")]
        public string CharacterId { get; set; } = string.Empty;

        [JsonPropertyName("stats")]
        public Stats Stats { get; set; } = new();

        [JsonPropertyName("roundDamage")]
        public List<RoundDamage> RoundDamage { get; set; } = new();

        [JsonPropertyName("competitiveTier")]
        public double CompetitiveTier { get; set; } = 0;

        [JsonPropertyName("playerCard")]
        public string PlayerCard { get; set; } = string.Empty;

        [JsonPropertyName("playerTitle")]
        public string PlayerTitle { get; set; } = string.Empty;

        [JsonPropertyName("preferredLevelBorder")]
        public string PreferredLevelBorder { get; set; } = string.Empty;

        [JsonPropertyName("accountLevel")]
        public double AccountLevel { get; set; } = 0;

        [JsonPropertyName("sessionPlaytimeMinutes")]
        public double SessionPlaytimeMinutes { get; set; } = 0;

        [JsonPropertyName("behaviorFactors")]
        public BehaviorFactors BehaviorFactors { get; set; } = new();

        [JsonPropertyName("newPlayerExperienceDetails")]
        public NewPlayerExperienceDetails NewPlayerExperienceDetails { get; set; } = new();

        [JsonPropertyName("xpModifications")]
        public List<XpModification> XpModifications { get; set; } = new();
    }

    public sealed class PlayerEconomy
    {
        [JsonPropertyName("subject")]
        public string Subject { get; set; } = string.Empty;

        [JsonPropertyName("loadoutValue")]
        public double LoadoutValue { get; set; } = 0;

        [JsonPropertyName("weapon")]
        public string Weapon { get; set; } = string.Empty;

        [JsonPropertyName("armor")]
        public string Armor { get; set; } = string.Empty;

        [JsonPropertyName("remaining")]
        public double Remaining { get; set; } = 0;

        [JsonPropertyName("spent")]
        public double Spent { get; set; } = 0;
    }

    public sealed class PlayerLocation
    {
        [JsonPropertyName("subject")]
        public string Subject { get; set; } = string.Empty;

        [JsonPropertyName("viewRadians")]
        public double ViewRadians { get; set; } = 0;

        [JsonPropertyName("location")]
        public Location Location { get; set; } = new();
    }

    public sealed class PlayerScore
    {
        [JsonPropertyName("subject")]
        public string Subject { get; set; } = string.Empty;

        [JsonPropertyName("score")]
        public double Score { get; set; } = 0;
    }

    public sealed class PlayerStat
    {
        [JsonPropertyName("subject")]
        public string Subject { get; set; } = string.Empty;

        [JsonPropertyName("kills")]
        public List<Kill> Kills { get; set; } = new();

        [JsonPropertyName("damage")]
        public List<Damage> Damage { get; set; } = new();

        [JsonPropertyName("score")]
        public double Score { get; set; } = 0;

        [JsonPropertyName("economy")]
        public Economy Economy { get; set; } = new();

        [JsonPropertyName("ability")]
        public Ability Ability { get; set; } = new();

        [JsonPropertyName("wasAfk")]
        public bool WasAfk { get; set; } = false;

        [JsonPropertyName("wasPenalized")]
        public bool WasPenalized { get; set; } = false;

        [JsonPropertyName("stayedInSpawn")]
        public bool StayedInSpawn { get; set; } = false;
    }

    public sealed class ValorantMatch
    {
        [JsonPropertyName("matchInfo")]
        public MatchInfo MatchInfo { get; set; } = new();

        [JsonPropertyName("players")]
        public List<Player> Players { get; set; } = new();

        [JsonPropertyName("bots")]
        public List<object> Bots { get; set; } = new();

        [JsonPropertyName("coaches")]
        public List<object> Coaches { get; set; } = new();

        [JsonPropertyName("teams")]
        public List<Team> Teams { get; set; } = new();

        [JsonPropertyName("roundResults")]
        public List<RoundResult> RoundResults { get; set; } = new();

        [JsonPropertyName("kills")]
        public List<Kill> Kills { get; set; } = new();
    }

    public sealed class RoundDamage
    {
        [JsonPropertyName("round")]
        public double Round { get; set; } = 0;

        [JsonPropertyName("receiver")]
        public string Receiver { get; set; } = string.Empty;

        [JsonPropertyName("damage")]
        public double Damage { get; set; } = 0;
    }

    public sealed class RoundResult
    {
        [JsonPropertyName("roundNum")]
        public double RoundNum { get; set; } = 0;

        [JsonPropertyName("roundResult")]
        public string RoundResults { get; set; } = string.Empty;

        [JsonPropertyName("roundCeremony")]
        public string RoundCeremony { get; set; } = string.Empty;

        [JsonPropertyName("winningTeam")]
        public string WinningTeam { get; set; } = string.Empty;

        [JsonPropertyName("bombPlanter")]
        public string BombPlanter { get; set; } = string.Empty;

        [JsonPropertyName("plantRoundTime")]
        public double PlantRoundTime { get; set; } = 0;

        [JsonPropertyName("plantPlayerLocations")]
        public List<PlantPlayerLocation> PlantPlayerLocations { get; set; } = new();

        [JsonPropertyName("plantLocation")]
        public PlantLocation PlantLocation { get; set; } = new();

        [JsonPropertyName("plantSite")]
        public string PlantSite { get; set; } = string.Empty;

        [JsonPropertyName("defuseRoundTime")]
        public double DefuseRoundTime { get; set; } = 0;

        [JsonPropertyName("defusePlayerLocations")]
        public List<DefusePlayerLocation> DefusePlayerLocations { get; set; } = new();

        [JsonPropertyName("defuseLocation")]
        public DefuseLocation DefuseLocation { get; set; } = new();

        [JsonPropertyName("playerStats")]
        public List<PlayerStat> PlayerStats { get; set; } = new();

        [JsonPropertyName("roundResultCode")]
        public string RoundResultCode { get; set; } = string.Empty;

        [JsonPropertyName("playerEconomies")]
        public List<PlayerEconomy> PlayerEconomies { get; set; } = new();

        [JsonPropertyName("playerScores")]
        public List<PlayerScore> PlayerScores { get; set; } = new();

        [JsonPropertyName("bombDefuser")]
        public string BombDefuser { get; set; } = string.Empty;
    }

    public sealed class SettingStatus
    {
        [JsonPropertyName("isMouseSensitivityDefault")]
        public bool IsMouseSensitivityDefault { get; set; }

        [JsonPropertyName("isCrosshairDefault")]
        public bool IsCrosshairDefault { get; set; }
    }

    public sealed class Stats
    {
        [JsonPropertyName("score")]
        public double Score { get; set; }

        [JsonPropertyName("roundsPlayed")]
        public double RoundsPlayed { get; set; }

        [JsonPropertyName("kills")]
        public double Kills { get; set; }

        [JsonPropertyName("deaths")]
        public double Deaths { get; set; }

        [JsonPropertyName("assists")]
        public double Assists { get; set; }

        [JsonPropertyName("playtimeMillis")]
        public double PlaytimeMillis { get; set; }

        [JsonPropertyName("abilityCasts")]
        public AbilityCasts AbilityCasts { get; set; } = new();
    }

    public sealed class Team
    {
        [JsonPropertyName("teamId")]
        public string TeamId { get; set; } = string.Empty;

        [JsonPropertyName("won")]
        public bool Won { get; set; }

        [JsonPropertyName("roundsPlayed")]
        public double RoundsPlayed { get; set; }

        [JsonPropertyName("roundsWon")]
        public double RoundsWon { get; set; }

        [JsonPropertyName("numPodoubles")]
        public double NumPodoubles { get; set; }
    }

    public sealed class VictimLocation
    {
        [JsonPropertyName("x")]
        public double X { get; set; }

        [JsonPropertyName("y")]
        public double Y { get; set; }
    }

    public sealed class XpModification
    {
        [JsonPropertyName("Value")]
        public double Value { get; set; } = 0;

        [JsonPropertyName("ID")]
        public string ID { get; set; } = string.Empty;
    }


}
