using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AccountManager.Core.Models.RiotGames.Valorant.Responses
{
    public class Ability
    {
        [JsonPropertyName("idleTimeMillis")]
        public double IdleTimeMillis { get; set; }

        [JsonPropertyName("objectiveCompleteTimeMillis")]
        public double ObjectiveCompleteTimeMillis { get; set; }

        [JsonPropertyName("grenadeEffects")]
        public object GrenadeEffects { get; set; }

        [JsonPropertyName("ability1Effects")]
        public object Ability1Effects { get; set; }

        [JsonPropertyName("ability2Effects")]
        public object Ability2Effects { get; set; }

        [JsonPropertyName("ultimateEffects")]
        public object UltimateEffects { get; set; }
    }

    public class AbilityCasts
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

    public class AdaptiveBots
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
        public object KillDetailsFirstAttempt { get; set; }
    }

    public class BasicGunSkill
    {
        [JsonPropertyName("idleTimeMillis")]
        public double IdleTimeMillis { get; set; }

        [JsonPropertyName("objectiveCompleteTimeMillis")]
        public double ObjectiveCompleteTimeMillis { get; set; }
    }

    public class BasicMovement
    {
        [JsonPropertyName("idleTimeMillis")]
        public double IdleTimeMillis { get; set; }

        [JsonPropertyName("objectiveCompleteTimeMillis")]
        public double ObjectiveCompleteTimeMillis { get; set; }
    }

    public class BehaviorFactors
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

    public class BombPlant
    {
        [JsonPropertyName("idleTimeMillis")]
        public double IdleTimeMillis { get; set; }

        [JsonPropertyName("objectiveCompleteTimeMillis")]
        public double ObjectiveCompleteTimeMillis { get; set; }
    }

    public class Damage
    {
        [JsonPropertyName("receiver")]
        public string Receiver { get; set; }

        [JsonPropertyName("damage")]
        public double DamageAmount { get; set; }

        [JsonPropertyName("legshots")]
        public double Legshots { get; set; }

        [JsonPropertyName("bodyshots")]
        public double Bodyshots { get; set; }

        [JsonPropertyName("headshots")]
        public double Headshots { get; set; }
    }

    public class DefendBombSite
    {
        [JsonPropertyName("idleTimeMillis")]
        public double IdleTimeMillis { get; set; }

        [JsonPropertyName("objectiveCompleteTimeMillis")]
        public double ObjectiveCompleteTimeMillis { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }
    }

    public class DefuseLocation
    {
        [JsonPropertyName("x")]
        public double X { get; set; }

        [JsonPropertyName("y")]
        public double Y { get; set; }
    }

    public class DefusePlayerLocation
    {
        [JsonPropertyName("subject")]
        public string Subject { get; set; }

        [JsonPropertyName("viewRadians")]
        public double ViewRadians { get; set; }

        [JsonPropertyName("location")]
        public Location Location { get; set; }
    }

    public class Economy
    {
        [JsonPropertyName("loadoutValue")]
        public double LoadoutValue { get; set; }

        [JsonPropertyName("weapon")]
        public string Weapon { get; set; }

        [JsonPropertyName("armor")]
        public string Armor { get; set; }

        [JsonPropertyName("remaining")]
        public double Remaining { get; set; }

        [JsonPropertyName("spent")]
        public double Spent { get; set; }
    }

    public class FinishingDamage
    {
        [JsonPropertyName("damageType")]
        public string DamageType { get; set; }

        [JsonPropertyName("damageItem")]
        public string DamageItem { get; set; }

        [JsonPropertyName("isSecondaryFireMode")]
        public bool IsSecondaryFireMode { get; set; }
    }

    public class Kill
    {
        [JsonPropertyName("gameTime")]
        public double GameTime { get; set; }

        [JsonPropertyName("roundTime")]
        public double RoundTime { get; set; }

        [JsonPropertyName("killer")]
        public string Killer { get; set; }

        [JsonPropertyName("victim")]
        public string Victim { get; set; }

        [JsonPropertyName("victimLocation")]
        public VictimLocation VictimLocation { get; set; }

        [JsonPropertyName("assistants")]
        public List<string> Assistants { get; set; }

        [JsonPropertyName("playerLocations")]
        public List<PlayerLocation> PlayerLocations { get; set; }

        [JsonPropertyName("finishingDamage")]
        public FinishingDamage FinishingDamage { get; set; }

        [JsonPropertyName("round")]
        public double Round { get; set; }
    }

    public class Location
    {
        [JsonPropertyName("x")]
        public double X { get; set; }

        [JsonPropertyName("y")]
        public double Y { get; set; }
    }

    public class MatchInfo
    {
        [JsonPropertyName("matchId")]
        public string MatchId { get; set; }

        [JsonPropertyName("mapId")]
        public string MapId { get; set; }

        [JsonPropertyName("gamePodId")]
        public string GamePodId { get; set; }

        [JsonPropertyName("gameLoopZone")]
        public string GameLoopZone { get; set; }

        [JsonPropertyName("gameServerAddress")]
        public string GameServerAddress { get; set; }

        [JsonPropertyName("gameVersion")]
        public string GameVersion { get; set; }

        [JsonPropertyName("gameLengthMillis")]
        public double GameLengthMillis { get; set; }

        [JsonPropertyName("gameStartMillis")]
        public long GameStartMillis { get; set; }

        [JsonPropertyName("provisioningFlowID")]
        public string ProvisioningFlowID { get; set; }

        [JsonPropertyName("isCompleted")]
        public bool IsCompleted { get; set; }

        [JsonPropertyName("customGameName")]
        public string CustomGameName { get; set; }

        [JsonPropertyName("forcePostProcessing")]
        public bool ForcePostProcessing { get; set; }

        [JsonPropertyName("queueID")]
        public string QueueID { get; set; }

        [JsonPropertyName("gameMode")]
        public string GameMode { get; set; }

        [JsonPropertyName("isRanked")]
        public bool IsRanked { get; set; }

        [JsonPropertyName("isMatchSampled")]
        public bool IsMatchSampled { get; set; }

        [JsonPropertyName("seasonId")]
        public string SeasonId { get; set; }

        [JsonPropertyName("completionState")]
        public string CompletionState { get; set; }

        [JsonPropertyName("platformType")]
        public string PlatformType { get; set; }

        [JsonPropertyName("partyRRPenalties")]
        public PartyRRPenalties PartyRRPenalties { get; set; }

        [JsonPropertyName("shouldMatchDisablePenalties")]
        public bool ShouldMatchDisablePenalties { get; set; }
    }

    public class NewPlayerExperienceDetails
    {
        [JsonPropertyName("basicMovement")]
        public BasicMovement BasicMovement { get; set; }

        [JsonPropertyName("basicGunSkill")]
        public BasicGunSkill BasicGunSkill { get; set; }

        [JsonPropertyName("adaptiveBots")]
        public AdaptiveBots AdaptiveBots { get; set; }

        [JsonPropertyName("ability")]
        public Ability Ability { get; set; }

        [JsonPropertyName("bombPlant")]
        public BombPlant BombPlant { get; set; }

        [JsonPropertyName("defendBombSite")]
        public DefendBombSite DefendBombSite { get; set; }

        [JsonPropertyName("settingStatus")]
        public SettingStatus SettingStatus { get; set; }
    }

    public class PartyRRPenalties
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

    public class PlantLocation
    {
        [JsonPropertyName("x")]
        public double X { get; set; }

        [JsonPropertyName("y")]
        public double Y { get; set; }
    }

    public class PlantPlayerLocation
    {
        [JsonPropertyName("subject")]
        public string Subject { get; set; }

        [JsonPropertyName("viewRadians")]
        public double ViewRadians { get; set; }

        [JsonPropertyName("location")]
        public Location Location { get; set; }
    }

    public class PlatformInfo
    {
        [JsonPropertyName("platformType")]
        public string PlatformType { get; set; }

        [JsonPropertyName("platformOS")]
        public string PlatformOS { get; set; }

        [JsonPropertyName("platformOSVersion")]
        public string PlatformOSVersion { get; set; }

        [JsonPropertyName("platformChipset")]
        public string PlatformChipset { get; set; }
    }

    public class Player
    {
        [JsonPropertyName("subject")]
        public string Subject { get; set; }

        [JsonPropertyName("gameName")]
        public string GameName { get; set; }

        [JsonPropertyName("tagLine")]
        public string TagLine { get; set; }

        [JsonPropertyName("platformInfo")]
        public PlatformInfo PlatformInfo { get; set; }

        [JsonPropertyName("teamId")]
        public string TeamId { get; set; }

        [JsonPropertyName("partyId")]
        public string PartyId { get; set; }

        [JsonPropertyName("characterId")]
        public string CharacterId { get; set; }

        [JsonPropertyName("stats")]
        public Stats Stats { get; set; }

        [JsonPropertyName("roundDamage")]
        public List<RoundDamage> RoundDamage { get; set; }

        [JsonPropertyName("competitiveTier")]
        public double CompetitiveTier { get; set; }

        [JsonPropertyName("playerCard")]
        public string PlayerCard { get; set; }

        [JsonPropertyName("playerTitle")]
        public string PlayerTitle { get; set; }

        [JsonPropertyName("preferredLevelBorder")]
        public string PreferredLevelBorder { get; set; }

        [JsonPropertyName("accountLevel")]
        public double AccountLevel { get; set; }

        [JsonPropertyName("sessionPlaytimeMinutes")]
        public double SessionPlaytimeMinutes { get; set; }

        [JsonPropertyName("behaviorFactors")]
        public BehaviorFactors BehaviorFactors { get; set; }

        [JsonPropertyName("newPlayerExperienceDetails")]
        public NewPlayerExperienceDetails NewPlayerExperienceDetails { get; set; }

        [JsonPropertyName("xpModifications")]
        public List<XpModification> XpModifications { get; set; }
    }

    public class PlayerEconomy
    {
        [JsonPropertyName("subject")]
        public string Subject { get; set; }

        [JsonPropertyName("loadoutValue")]
        public double LoadoutValue { get; set; }

        [JsonPropertyName("weapon")]
        public string Weapon { get; set; }

        [JsonPropertyName("armor")]
        public string Armor { get; set; }

        [JsonPropertyName("remaining")]
        public double Remaining { get; set; }

        [JsonPropertyName("spent")]
        public double Spent { get; set; }
    }

    public class PlayerLocation
    {
        [JsonPropertyName("subject")]
        public string Subject { get; set; }

        [JsonPropertyName("viewRadians")]
        public double ViewRadians { get; set; }

        [JsonPropertyName("location")]
        public Location Location { get; set; }
    }

    public class PlayerScore
    {
        [JsonPropertyName("subject")]
        public string Subject { get; set; }

        [JsonPropertyName("score")]
        public double Score { get; set; }
    }

    public class PlayerStat
    {
        [JsonPropertyName("subject")]
        public string Subject { get; set; }

        [JsonPropertyName("kills")]
        public List<Kill> Kills { get; set; }

        [JsonPropertyName("damage")]
        public List<Damage> Damage { get; set; }

        [JsonPropertyName("score")]
        public double Score { get; set; }

        [JsonPropertyName("economy")]
        public Economy Economy { get; set; }

        [JsonPropertyName("ability")]
        public Ability Ability { get; set; }

        [JsonPropertyName("wasAfk")]
        public bool WasAfk { get; set; }

        [JsonPropertyName("wasPenalized")]
        public bool WasPenalized { get; set; }

        [JsonPropertyName("stayedInSpawn")]
        public bool StayedInSpawn { get; set; }
    }

    public class ValorantMatch
    {
        [JsonPropertyName("matchInfo")]
        public MatchInfo MatchInfo { get; set; }

        [JsonPropertyName("players")]
        public List<Player> Players { get; set; }

        [JsonPropertyName("bots")]
        public List<object> Bots { get; set; }

        [JsonPropertyName("coaches")]
        public List<object> Coaches { get; set; }

        [JsonPropertyName("teams")]
        public List<Team> Teams { get; set; }

        [JsonPropertyName("roundResults")]
        public List<RoundResult> RoundResults { get; set; }

        [JsonPropertyName("kills")]
        public List<Kill> Kills { get; set; }
    }

    public class RoundDamage
    {
        [JsonPropertyName("round")]
        public double Round { get; set; }

        [JsonPropertyName("receiver")]
        public string Receiver { get; set; }

        [JsonPropertyName("damage")]
        public double Damage { get; set; }
    }

    public class RoundResult
    {
        [JsonPropertyName("roundNum")]
        public double RoundNum { get; set; }

        [JsonPropertyName("roundResult")]
        public string RoundResults { get; set; }

        [JsonPropertyName("roundCeremony")]
        public string RoundCeremony { get; set; }

        [JsonPropertyName("winningTeam")]
        public string WinningTeam { get; set; }

        [JsonPropertyName("bombPlanter")]
        public string BombPlanter { get; set; }

        [JsonPropertyName("plantRoundTime")]
        public double PlantRoundTime { get; set; }

        [JsonPropertyName("plantPlayerLocations")]
        public List<PlantPlayerLocation> PlantPlayerLocations { get; set; }

        [JsonPropertyName("plantLocation")]
        public PlantLocation PlantLocation { get; set; }

        [JsonPropertyName("plantSite")]
        public string PlantSite { get; set; }

        [JsonPropertyName("defuseRoundTime")]
        public double DefuseRoundTime { get; set; }

        [JsonPropertyName("defusePlayerLocations")]
        public List<DefusePlayerLocation> DefusePlayerLocations { get; set; }

        [JsonPropertyName("defuseLocation")]
        public DefuseLocation DefuseLocation { get; set; }

        [JsonPropertyName("playerStats")]
        public List<PlayerStat> PlayerStats { get; set; }

        [JsonPropertyName("roundResultCode")]
        public string RoundResultCode { get; set; }

        [JsonPropertyName("playerEconomies")]
        public List<PlayerEconomy> PlayerEconomies { get; set; }

        [JsonPropertyName("playerScores")]
        public List<PlayerScore> PlayerScores { get; set; }

        [JsonPropertyName("bombDefuser")]
        public string BombDefuser { get; set; }
    }

    public class SettingStatus
    {
        [JsonPropertyName("isMouseSensitivityDefault")]
        public bool IsMouseSensitivityDefault { get; set; }

        [JsonPropertyName("isCrosshairDefault")]
        public bool IsCrosshairDefault { get; set; }
    }

    public class Stats
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
        public AbilityCasts AbilityCasts { get; set; }
    }

    public class Team
    {
        [JsonPropertyName("teamId")]
        public string TeamId { get; set; }

        [JsonPropertyName("won")]
        public bool Won { get; set; }

        [JsonPropertyName("roundsPlayed")]
        public double RoundsPlayed { get; set; }

        [JsonPropertyName("roundsWon")]
        public double RoundsWon { get; set; }

        [JsonPropertyName("numPodoubles")]
        public double NumPodoubles { get; set; }
    }

    public class VictimLocation
    {
        [JsonPropertyName("x")]
        public double X { get; set; }

        [JsonPropertyName("y")]
        public double Y { get; set; }
    }

    public class XpModification
    {
        [JsonPropertyName("Value")]
        public double Value { get; set; }

        [JsonPropertyName("ID")]
        public string ID { get; set; }
    }


}
