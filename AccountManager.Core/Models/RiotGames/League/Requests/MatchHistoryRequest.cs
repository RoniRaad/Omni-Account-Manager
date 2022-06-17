using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AccountManager.Core.Models.RiotGames.League.Requests
{
    public class MatchHistoryResponse
    {
        [JsonPropertyName("games")]
        public List<Game>? Games { get; set; }

        public class Ban
        {
            [JsonPropertyName("championId")]
            public int ChampionId { get; set; }

            [JsonPropertyName("pickTurn")]
            public int PickTurn { get; set; }
        }

        public class Baron
        {
            [JsonPropertyName("first")]
            public bool First { get; set; }

            [JsonPropertyName("kills")]
            public int Kills { get; set; }
        }

        public class Challenges
        {
            [JsonPropertyName("12AssistStreakCount")]
            public double _12AssistStreakCount { get; set; }

            [JsonPropertyName("abilityUses")]
            public double AbilityUses { get; set; }

            [JsonPropertyName("acesBefore15Minutes")]
            public double AcesBefore15Minutes { get; set; }

            [JsonPropertyName("alliedJungleMonsterKills")]
            public double AlliedJungleMonsterKills { get; set; }

            [JsonPropertyName("baronTakedowns")]
            public double BaronTakedowns { get; set; }

            [JsonPropertyName("blastConeOppositeOpponentCount")]
            public double BlastConeOppositeOpponentCount { get; set; }

            [JsonPropertyName("bountyGold")]
            public double BountyGold { get; set; }

            [JsonPropertyName("buffsStolen")]
            public double BuffsStolen { get; set; }

            [JsonPropertyName("completeSupportQuestInTime")]
            public double CompleteSupportQuestInTime { get; set; }

            [JsonPropertyName("controlWardsPlaced")]
            public double ControlWardsPlaced { get; set; }

            [JsonPropertyName("damagePerMinute")]
            public double DamagePerMinute { get; set; }

            [JsonPropertyName("damageTakenOnTeamPercentage")]
            public double DamageTakenOnTeamPercentage { get; set; }

            [JsonPropertyName("dancedWithRiftHerald")]
            public double DancedWithRiftHerald { get; set; }

            [JsonPropertyName("deathsByEnemyChamps")]
            public double DeathsByEnemyChamps { get; set; }

            [JsonPropertyName("dodgeSkillShotsSmallWindow")]
            public double DodgeSkillShotsSmallWindow { get; set; }

            [JsonPropertyName("doubleAces")]
            public double DoubleAces { get; set; }

            [JsonPropertyName("dragonTakedowns")]
            public double DragonTakedowns { get; set; }

            [JsonPropertyName("earliestBaron")]
            public double EarliestBaron { get; set; }

            [JsonPropertyName("earlyLaningPhaseGoldExpAdvantage")]
            public double EarlyLaningPhaseGoldExpAdvantage { get; set; }

            [JsonPropertyName("effectiveHealAndShielding")]
            public double EffectiveHealAndShielding { get; set; }

            [JsonPropertyName("elderDragonKillsWithOpposingSoul")]
            public double ElderDragonKillsWithOpposingSoul { get; set; }

            [JsonPropertyName("elderDragonMultikills")]
            public double ElderDragonMultikills { get; set; }

            [JsonPropertyName("enemyChampionImmobilizations")]
            public double EnemyChampionImmobilizations { get; set; }

            [JsonPropertyName("enemyJungleMonsterKills")]
            public double EnemyJungleMonsterKills { get; set; }

            [JsonPropertyName("epicMonsterKillsNearEnemyJungler")]
            public double EpicMonsterKillsNearEnemyJungler { get; set; }

            [JsonPropertyName("epicMonsterKillsWithin30SecondsOfSpawn")]
            public double EpicMonsterKillsWithin30SecondsOfSpawn { get; set; }

            [JsonPropertyName("epicMonsterSteals")]
            public double EpicMonsterSteals { get; set; }

            [JsonPropertyName("epicMonsterStolenWithoutSmite")]
            public double EpicMonsterStolenWithoutSmite { get; set; }

            [JsonPropertyName("flawlessAces")]
            public double FlawlessAces { get; set; }

            [JsonPropertyName("fullTeamTakedown")]
            public double FullTeamTakedown { get; set; }

            [JsonPropertyName("gameLength")]
            public double GameLength { get; set; }

            [JsonPropertyName("getTakedownsInAllLanesEarlyJungleAsLaner")]
            public double GetTakedownsInAllLanesEarlyJungleAsLaner { get; set; }

            [JsonPropertyName("goldPerMinute")]
            public double GoldPerMinute { get; set; }

            [JsonPropertyName("hadAfkTeammate")]
            public double HadAfkTeammate { get; set; }

            [JsonPropertyName("hadOpenNexus")]
            public double HadOpenNexus { get; set; }

            [JsonPropertyName("immobilizeAndKillWithAlly")]
            public double ImmobilizeAndKillWithAlly { get; set; }

            [JsonPropertyName("initialBuffCount")]
            public double InitialBuffCount { get; set; }

            [JsonPropertyName("initialCrabCount")]
            public double InitialCrabCount { get; set; }

            [JsonPropertyName("jungleCsBefore10Minutes")]
            public double JungleCsBefore10Minutes { get; set; }

            [JsonPropertyName("junglerTakedownsNearDamagedEpicMonster")]
            public double JunglerTakedownsNearDamagedEpicMonster { get; set; }

            [JsonPropertyName("kTurretsDestroyedBeforePlatesFall")]
            public double KTurretsDestroyedBeforePlatesFall { get; set; }

            [JsonPropertyName("kda")]
            public double Kda { get; set; }

            [JsonPropertyName("killAfterHiddenWithAlly")]
            public double KillAfterHiddenWithAlly { get; set; }

            [JsonPropertyName("killParticipation")]
            public double KillParticipation { get; set; }

            [JsonPropertyName("killedChampTookFullTeamDamageSurvived")]
            public double KilledChampTookFullTeamDamageSurvived { get; set; }

            [JsonPropertyName("killsNearEnemyTurret")]
            public double KillsNearEnemyTurret { get; set; }

            [JsonPropertyName("killsOnOtherLanesEarlyJungleAsLaner")]
            public double KillsOnOtherLanesEarlyJungleAsLaner { get; set; }

            [JsonPropertyName("killsOnRecentlyHealedByAramPack")]
            public double KillsOnRecentlyHealedByAramPack { get; set; }

            [JsonPropertyName("killsUnderOwnTurret")]
            public double KillsUnderOwnTurret { get; set; }

            [JsonPropertyName("killsWithHelpFromEpicMonster")]
            public double KillsWithHelpFromEpicMonster { get; set; }

            [JsonPropertyName("knockEnemyIntoTeamAndKill")]
            public double KnockEnemyIntoTeamAndKill { get; set; }

            [JsonPropertyName("landSkillShotsEarlyGame")]
            public double LandSkillShotsEarlyGame { get; set; }

            [JsonPropertyName("laneMinionsFirst10Minutes")]
            public double LaneMinionsFirst10Minutes { get; set; }

            [JsonPropertyName("laningPhaseGoldExpAdvantage")]
            public double LaningPhaseGoldExpAdvantage { get; set; }

            [JsonPropertyName("legendaryCount")]
            public double LegendaryCount { get; set; }

            [JsonPropertyName("lostAnInhibitor")]
            public double LostAnInhibitor { get; set; }

            [JsonPropertyName("maxCsAdvantageOnLaneOpponent")]
            public double MaxCsAdvantageOnLaneOpponent { get; set; }

            [JsonPropertyName("maxKillDeficit")]
            public double MaxKillDeficit { get; set; }

            [JsonPropertyName("maxLevelLeadLaneOpponent")]
            public double MaxLevelLeadLaneOpponent { get; set; }

            [JsonPropertyName("moreEnemyJungleThanOpponent")]
            public double MoreEnemyJungleThanOpponent { get; set; }

            [JsonPropertyName("multiKillOneSpell")]
            public double MultiKillOneSpell { get; set; }

            [JsonPropertyName("multiTurretRiftHeraldCount")]
            public double MultiTurretRiftHeraldCount { get; set; }

            [JsonPropertyName("multikills")]
            public double Multikills { get; set; }

            [JsonPropertyName("multikillsAfterAggressiveFlash")]
            public double MultikillsAfterAggressiveFlash { get; set; }

            [JsonPropertyName("outerTurretExecutesBefore10Minutes")]
            public double OuterTurretExecutesBefore10Minutes { get; set; }

            [JsonPropertyName("outnumberedKills")]
            public double OutnumberedKills { get; set; }

            [JsonPropertyName("outnumberedNexusKill")]
            public double OutnumberedNexusKill { get; set; }

            [JsonPropertyName("perfectDragonSoulsTaken")]
            public double PerfectDragonSoulsTaken { get; set; }

            [JsonPropertyName("perfectGame")]
            public double PerfectGame { get; set; }

            [JsonPropertyName("pickKillWithAlly")]
            public double PickKillWithAlly { get; set; }

            [JsonPropertyName("poroExplosions")]
            public double PoroExplosions { get; set; }

            [JsonPropertyName("quickCleanse")]
            public double QuickCleanse { get; set; }

            [JsonPropertyName("quickFirstTurret")]
            public double QuickFirstTurret { get; set; }

            [JsonPropertyName("quickSoloKills")]
            public double QuickSoloKills { get; set; }

            [JsonPropertyName("riftHeraldTakedowns")]
            public double RiftHeraldTakedowns { get; set; }

            [JsonPropertyName("saveAllyFromDeath")]
            public double SaveAllyFromDeath { get; set; }

            [JsonPropertyName("scuttleCrabKills")]
            public double ScuttleCrabKills { get; set; }

            [JsonPropertyName("skillshotsDodged")]
            public double SkillshotsDodged { get; set; }

            [JsonPropertyName("skillshotsHit")]
            public double SkillshotsHit { get; set; }

            [JsonPropertyName("snowballsHit")]
            public double SnowballsHit { get; set; }

            [JsonPropertyName("soloBaronKills")]
            public double SoloBaronKills { get; set; }

            [JsonPropertyName("soloKills")]
            public double SoloKills { get; set; }

            [JsonPropertyName("soloTurretsLategame")]
            public double SoloTurretsLategame { get; set; }

            [JsonPropertyName("stealthWardsPlaced")]
            public double StealthWardsPlaced { get; set; }

            [JsonPropertyName("survivedSingleDigitHpCount")]
            public double SurvivedSingleDigitHpCount { get; set; }

            [JsonPropertyName("survivedThreeImmobilizesInFight")]
            public double SurvivedThreeImmobilizesInFight { get; set; }

            [JsonPropertyName("takedownOnFirstTurret")]
            public double TakedownOnFirstTurret { get; set; }

            [JsonPropertyName("takedowns")]
            public double Takedowns { get; set; }

            [JsonPropertyName("takedownsAfterGainingLevelAdvantage")]
            public double TakedownsAfterGainingLevelAdvantage { get; set; }

            [JsonPropertyName("takedownsBeforeJungleMinionSpawn")]
            public double TakedownsBeforeJungleMinionSpawn { get; set; }

            [JsonPropertyName("takedownsFirstXMinutes")]
            public double TakedownsFirstXMinutes { get; set; }

            [JsonPropertyName("takedownsInAlcove")]
            public double TakedownsInAlcove { get; set; }

            [JsonPropertyName("takedownsInEnemyFountain")]
            public double TakedownsInEnemyFountain { get; set; }

            [JsonPropertyName("teamBaronKills")]
            public double TeamBaronKills { get; set; }

            [JsonPropertyName("teamDamagePercentage")]
            public double TeamDamagePercentage { get; set; }

            [JsonPropertyName("teamElderDragonKills")]
            public double TeamElderDragonKills { get; set; }

            [JsonPropertyName("teamRiftHeraldKills")]
            public double TeamRiftHeraldKills { get; set; }

            [JsonPropertyName("threeWardsOneSweeperCount")]
            public double ThreeWardsOneSweeperCount { get; set; }

            [JsonPropertyName("tookLargeDamageSurvived")]
            public double TookLargeDamageSurvived { get; set; }

            [JsonPropertyName("turretPlatesTaken")]
            public double TurretPlatesTaken { get; set; }

            [JsonPropertyName("turretTakedowns")]
            public double TurretTakedowns { get; set; }

            [JsonPropertyName("turretsTakenWithRiftHerald")]
            public double TurretsTakenWithRiftHerald { get; set; }

            [JsonPropertyName("twentyMinionsIn3SecondsCount")]
            public double TwentyMinionsIn3SecondsCount { get; set; }

            [JsonPropertyName("unseenRecalls")]
            public double UnseenRecalls { get; set; }

            [JsonPropertyName("visionScoreAdvantageLaneOpponent")]
            public double VisionScoreAdvantageLaneOpponent { get; set; }

            [JsonPropertyName("visionScorePerMinute")]
            public double VisionScorePerMinute { get; set; }

            [JsonPropertyName("wardTakedowns")]
            public double WardTakedowns { get; set; }

            [JsonPropertyName("wardTakedownsBefore20M")]
            public double WardTakedownsBefore20M { get; set; }

            [JsonPropertyName("wardsGuarded")]
            public double WardsGuarded { get; set; }

            [JsonPropertyName("earliestDragonTakedown")]
            public double? EarliestDragonTakedown { get; set; }

            [JsonPropertyName("highestChampionDamage")]
            public double? HighestChampionDamage { get; set; }

            [JsonPropertyName("junglerKillsEarlyJungle")]
            public double? JunglerKillsEarlyJungle { get; set; }

            [JsonPropertyName("killingSprees")]
            public double? KillingSprees { get; set; }

            [JsonPropertyName("killsOnLanersEarlyJungleAsJungler")]
            public double? KillsOnLanersEarlyJungleAsJungler { get; set; }

            [JsonPropertyName("mythicItemUsed")]
            public double? MythicItemUsed { get; set; }

            [JsonPropertyName("shortestTimeToAceFromFirstTakedown")]
            public double? ShortestTimeToAceFromFirstTakedown { get; set; }

            [JsonPropertyName("controlWardTimeCoverageInRiverOrEnemyHalf")]
            public double? ControlWardTimeCoverageInRiverOrEnemyHalf { get; set; }

            [JsonPropertyName("highestWardKills")]
            public double? HighestWardKills { get; set; }

            [JsonPropertyName("firstTurretKilledTime")]
            public double? FirstTurretKilledTime { get; set; }

            [JsonPropertyName("highestCrowdControlScore")]
            public double? HighestCrowdControlScore { get; set; }

            [JsonPropertyName("fasterSupportQuestCompletion")]
            public double? FasterSupportQuestCompletion { get; set; }

            [JsonPropertyName("fastestLegendary")]
            public double? FastestLegendary { get; set; }

            [JsonPropertyName("thirdInhibitorDestroyedTime")]
            public double? ThirdInhibitorDestroyedTime { get; set; }

            [JsonPropertyName("baronBuffGoldAdvantageOverThreshold")]
            public double? BaronBuffGoldAdvantageOverThreshold { get; set; }

            [JsonPropertyName("teleportTakedowns")]
            public double? TeleportTakedowns { get; set; }

            [JsonPropertyName("takedownsFirst25Minutes")]
            public double? TakedownsFirst25Minutes { get; set; }

            [JsonPropertyName("mejaisFullStackInTime")]
            public double? MejaisFullStackInTime { get; set; }
        }

        public class Champion
        {
            [JsonPropertyName("first")]
            public bool First { get; set; }

            [JsonPropertyName("kills")]
            public int Kills { get; set; }
        }

        public class Dragon
        {
            [JsonPropertyName("first")]
            public bool First { get; set; }

            [JsonPropertyName("kills")]
            public int Kills { get; set; }
        }

        public class Game
        {
            [JsonPropertyName("metadata")]
            public Metadata? Metadata { get; set; }

            [JsonPropertyName("json")]
            public Json? Json { get; set; }
        }

        public class Inhibitor
        {
            [JsonPropertyName("first")]
            public bool First { get; set; }

            [JsonPropertyName("kills")]
            public int Kills { get; set; }
        }

        public class Json
        {
            [JsonPropertyName("gameCreation")]
            public long? GameCreation { get; set; }

            [JsonPropertyName("gameDuration")]
            public int GameDuration { get; set; }

            [JsonPropertyName("gameEndTimestamp")]
            public long GameEndTimestamp { get; set; }

            [JsonPropertyName("gameId")]
            public object? GameId { get; set; }

            [JsonPropertyName("gameMode")]
            public string? GameMode { get; set; }

            [JsonPropertyName("gameName")]
            public string? GameName { get; set; }

            [JsonPropertyName("gameStartTimestamp")]
            public object? GameStartTimestamp { get; set; }

            [JsonPropertyName("gameType")]
            public string? GameType { get; set; }

            [JsonPropertyName("gameVersion")]
            public string? GameVersion { get; set; }

            [JsonPropertyName("mapId")]
            public int MapId { get; set; }

            [JsonPropertyName("participants")]
            public List<Participant>? Participants { get; set; }

            [JsonPropertyName("platformId")]
            public string? PlatformId { get; set; }

            [JsonPropertyName("queueId")]
            public int QueueId { get; set; }

            [JsonPropertyName("seasonId")]
            public int SeasonId { get; set; }

            [JsonPropertyName("teams")]
            public List<Team>? Teams { get; set; }

            [JsonPropertyName("tournamentCode")]
            public string? TournamentCode { get; set; }
        }

        public class Metadata
        {
            [JsonPropertyName("product")]
            public string? Product { get; set; }

            [JsonPropertyName("tags")]
            public List<string>? Tags { get; set; }

            [JsonPropertyName("participants")]
            public List<string>? Participants { get; set; }

            [JsonPropertyName("timestamp")]
            public string? Timestamp { get; set; }

            [JsonPropertyName("data_version")]
            public string? DataVersion { get; set; }

            [JsonPropertyName("info_type")]
            public string? InfoType { get; set; }

            [JsonPropertyName("match_id")]
            public string? MatchId { get; set; }

            [JsonPropertyName("private")]
            public bool Private { get; set; }
        }

        public class Objectives
        {
            [JsonPropertyName("baron")]
            public Baron? Baron { get; set; }

            [JsonPropertyName("champion")]
            public Champion? Champion { get; set; }

            [JsonPropertyName("dragon")]
            public Dragon? Dragon { get; set; }

            [JsonPropertyName("inhibitor")]
            public Inhibitor? Inhibitor { get; set; }

            [JsonPropertyName("riftHerald")]
            public RiftHerald? RiftHerald { get; set; }

            [JsonPropertyName("tower")]
            public Tower? Tower { get; set; }
        }

        public class Participant
        {
            [JsonPropertyName("assists")]
            public int Assists { get; set; }

            [JsonPropertyName("baronKills")]
            public int BaronKills { get; set; }

            [JsonPropertyName("bountyLevel")]
            public int BountyLevel { get; set; }

            [JsonPropertyName("challenges")]
            public Challenges? Challenges { get; set; }

            [JsonPropertyName("champExperience")]
            public int ChampExperience { get; set; }

            [JsonPropertyName("champLevel")]
            public int ChampLevel { get; set; }

            [JsonPropertyName("championId")]
            public int ChampionId { get; set; }

            [JsonPropertyName("championName")]
            public string? ChampionName { get; set; }

            [JsonPropertyName("championTransform")]
            public int ChampionTransform { get; set; }

            [JsonPropertyName("consumablesPurchased")]
            public int ConsumablesPurchased { get; set; }

            [JsonPropertyName("damageDealtToBuildings")]
            public int DamageDealtToBuildings { get; set; }

            [JsonPropertyName("damageDealtToObjectives")]
            public int DamageDealtToObjectives { get; set; }

            [JsonPropertyName("damageDealtToTurrets")]
            public int DamageDealtToTurrets { get; set; }

            [JsonPropertyName("damageSelfMitigated")]
            public int DamageSelfMitigated { get; set; }

            [JsonPropertyName("deaths")]
            public int Deaths { get; set; }

            [JsonPropertyName("detectorWardsPlaced")]
            public int DetectorWardsPlaced { get; set; }

            [JsonPropertyName("doubleKills")]
            public int DoubleKills { get; set; }

            [JsonPropertyName("dragonKills")]
            public int DragonKills { get; set; }

            [JsonPropertyName("eligibleForProgression")]
            public bool EligibleForProgression { get; set; }

            [JsonPropertyName("firstBloodAssist")]
            public bool FirstBloodAssist { get; set; }

            [JsonPropertyName("firstBloodKill")]
            public bool FirstBloodKill { get; set; }

            [JsonPropertyName("firstTowerAssist")]
            public bool FirstTowerAssist { get; set; }

            [JsonPropertyName("firstTowerKill")]
            public bool FirstTowerKill { get; set; }

            [JsonPropertyName("gameEndedInEarlySurrender")]
            public bool GameEndedInEarlySurrender { get; set; }

            [JsonPropertyName("gameEndedInSurrender")]
            public bool GameEndedInSurrender { get; set; }

            [JsonPropertyName("goldEarned")]
            public int GoldEarned { get; set; }

            [JsonPropertyName("goldSpent")]
            public int GoldSpent { get; set; }

            [JsonPropertyName("individualPosition")]
            public string? IndividualPosition { get; set; }

            [JsonPropertyName("inhibitorKills")]
            public int InhibitorKills { get; set; }

            [JsonPropertyName("inhibitorTakedowns")]
            public int InhibitorTakedowns { get; set; }

            [JsonPropertyName("inhibitorsLost")]
            public int InhibitorsLost { get; set; }

            [JsonPropertyName("item0")]
            public int Item0 { get; set; }

            [JsonPropertyName("item1")]
            public int Item1 { get; set; }

            [JsonPropertyName("item2")]
            public int Item2 { get; set; }

            [JsonPropertyName("item3")]
            public int Item3 { get; set; }

            [JsonPropertyName("item4")]
            public int Item4 { get; set; }

            [JsonPropertyName("item5")]
            public int Item5 { get; set; }

            [JsonPropertyName("item6")]
            public int Item6 { get; set; }

            [JsonPropertyName("itemsPurchased")]
            public int ItemsPurchased { get; set; }

            [JsonPropertyName("killingSprees")]
            public int KillingSprees { get; set; }

            [JsonPropertyName("kills")]
            public int Kills { get; set; }

            [JsonPropertyName("lane")]
            public string? Lane { get; set; }

            [JsonPropertyName("largestCriticalStrike")]
            public int LargestCriticalStrike { get; set; }

            [JsonPropertyName("largestKillingSpree")]
            public int LargestKillingSpree { get; set; }

            [JsonPropertyName("largestMultiKill")]
            public int LargestMultiKill { get; set; }

            [JsonPropertyName("longestTimeSpentLiving")]
            public int LongestTimeSpentLiving { get; set; }

            [JsonPropertyName("magicDamageDealt")]
            public int MagicDamageDealt { get; set; }

            [JsonPropertyName("magicDamageDealtToChampions")]
            public int MagicDamageDealtToChampions { get; set; }

            [JsonPropertyName("magicDamageTaken")]
            public int MagicDamageTaken { get; set; }

            [JsonPropertyName("neutralMinionsKilled")]
            public int NeutralMinionsKilled { get; set; }

            [JsonPropertyName("nexusKills")]
            public int NexusKills { get; set; }

            [JsonPropertyName("nexusLost")]
            public int NexusLost { get; set; }

            [JsonPropertyName("nexusTakedowns")]
            public int NexusTakedowns { get; set; }

            [JsonPropertyName("objectivesStolen")]
            public int ObjectivesStolen { get; set; }

            [JsonPropertyName("objectivesStolenAssists")]
            public int ObjectivesStolenAssists { get; set; }

            [JsonPropertyName("participantId")]
            public int ParticipantId { get; set; }

            [JsonPropertyName("pentaKills")]
            public int PentaKills { get; set; }

            [JsonPropertyName("perks")]
            public Perks? Perks { get; set; }

            [JsonPropertyName("physicalDamageDealt")]
            public int PhysicalDamageDealt { get; set; }

            [JsonPropertyName("physicalDamageDealtToChampions")]
            public int PhysicalDamageDealtToChampions { get; set; }

            [JsonPropertyName("physicalDamageTaken")]
            public int PhysicalDamageTaken { get; set; }

            [JsonPropertyName("profileIcon")]
            public int ProfileIcon { get; set; }

            [JsonPropertyName("puuid")]
            public string? Puuid { get; set; }

            [JsonPropertyName("quadraKills")]
            public int QuadraKills { get; set; }

            [JsonPropertyName("riotIdName")]
            public string? RiotIdName { get; set; }

            [JsonPropertyName("riotIdTagline")]
            public string? RiotIdTagline { get; set; }

            [JsonPropertyName("role")]
            public string? Role { get; set; }

            [JsonPropertyName("sightWardsBoughtInGame")]
            public int SightWardsBoughtInGame { get; set; }

            [JsonPropertyName("spell1Casts")]
            public int Spell1Casts { get; set; }

            [JsonPropertyName("spell1Id")]
            public int Spell1Id { get; set; }

            [JsonPropertyName("spell2Casts")]
            public int Spell2Casts { get; set; }

            [JsonPropertyName("spell2Id")]
            public int Spell2Id { get; set; }

            [JsonPropertyName("spell3Casts")]
            public int Spell3Casts { get; set; }

            [JsonPropertyName("spell4Casts")]
            public int Spell4Casts { get; set; }

            [JsonPropertyName("summoner1Casts")]
            public int Summoner1Casts { get; set; }

            [JsonPropertyName("summoner2Casts")]
            public int Summoner2Casts { get; set; }

            [JsonPropertyName("summonerId")]
            public object? SummonerId { get; set; }

            [JsonPropertyName("summonerLevel")]
            public int SummonerLevel { get; set; }

            [JsonPropertyName("summonerName")]
            public string? SummonerName { get; set; }

            [JsonPropertyName("teamEarlySurrendered")]
            public bool TeamEarlySurrendered { get; set; }

            [JsonPropertyName("teamId")]
            public int TeamId { get; set; }

            [JsonPropertyName("teamPosition")]
            public string? TeamPosition { get; set; }

            [JsonPropertyName("timeCCingOthers")]
            public int TimeCCingOthers { get; set; }

            [JsonPropertyName("timePlayed")]
            public int TimePlayed { get; set; }

            [JsonPropertyName("totalDamageDealt")]
            public int TotalDamageDealt { get; set; }

            [JsonPropertyName("totalDamageDealtToChampions")]
            public int TotalDamageDealtToChampions { get; set; }

            [JsonPropertyName("totalDamageShieldedOnTeammates")]
            public int TotalDamageShieldedOnTeammates { get; set; }

            [JsonPropertyName("totalDamageTaken")]
            public int TotalDamageTaken { get; set; }

            [JsonPropertyName("totalHeal")]
            public int TotalHeal { get; set; }

            [JsonPropertyName("totalHealsOnTeammates")]
            public int TotalHealsOnTeammates { get; set; }

            [JsonPropertyName("totalMinionsKilled")]
            public int TotalMinionsKilled { get; set; }

            [JsonPropertyName("totalTimeCCDealt")]
            public int TotalTimeCCDealt { get; set; }

            [JsonPropertyName("totalTimeSpentDead")]
            public int TotalTimeSpentDead { get; set; }

            [JsonPropertyName("totalUnitsHealed")]
            public int TotalUnitsHealed { get; set; }

            [JsonPropertyName("tripleKills")]
            public int TripleKills { get; set; }

            [JsonPropertyName("trueDamageDealt")]
            public int TrueDamageDealt { get; set; }

            [JsonPropertyName("trueDamageDealtToChampions")]
            public int TrueDamageDealtToChampions { get; set; }

            [JsonPropertyName("trueDamageTaken")]
            public int TrueDamageTaken { get; set; }

            [JsonPropertyName("turretKills")]
            public int TurretKills { get; set; }

            [JsonPropertyName("turretTakedowns")]
            public int TurretTakedowns { get; set; }

            [JsonPropertyName("turretsLost")]
            public int TurretsLost { get; set; }

            [JsonPropertyName("unrealKills")]
            public int UnrealKills { get; set; }

            [JsonPropertyName("visionScore")]
            public int VisionScore { get; set; }

            [JsonPropertyName("visionWardsBoughtInGame")]
            public int VisionWardsBoughtInGame { get; set; }

            [JsonPropertyName("wardsKilled")]
            public int WardsKilled { get; set; }

            [JsonPropertyName("wardsPlaced")]
            public int WardsPlaced { get; set; }

            [JsonPropertyName("win")]
            public bool Win { get; set; }
        }

        public class Perks
        {
            [JsonPropertyName("statPerks")]
            public StatPerks? StatPerks { get; set; }

            [JsonPropertyName("styles")]
            public List<Style>? Styles { get; set; }
        }

        public class RiftHerald
        {
            [JsonPropertyName("first")]
            public bool First { get; set; }

            [JsonPropertyName("kills")]
            public int Kills { get; set; }
        }

        public class Selection
        {
            [JsonPropertyName("perk")]
            public int Perk { get; set; }

            [JsonPropertyName("var1")]
            public int Var1 { get; set; }

            [JsonPropertyName("var2")]
            public int Var2 { get; set; }

            [JsonPropertyName("var3")]
            public int Var3 { get; set; }
        }

        public class StatPerks
        {
            [JsonPropertyName("defense")]
            public int Defense { get; set; }

            [JsonPropertyName("flex")]
            public int Flex { get; set; }

            [JsonPropertyName("offense")]
            public int Offense { get; set; }
        }

        public class Style
        {
            [JsonPropertyName("description")]
            public string? Description { get; set; }

            [JsonPropertyName("selections")]
            public List<Selection>? Selections { get; set; }

            [JsonPropertyName("style")]
            public int InnerStyle { get; set; }
        }

        public class Team
        {
            [JsonPropertyName("bans")]
            public List<Ban>? Bans { get; set; }

            [JsonPropertyName("objectives")]
            public Objectives? Objectives { get; set; }

            [JsonPropertyName("teamId")]
            public int TeamId { get; set; }

            [JsonPropertyName("win")]
            public bool Win { get; set; }
        }

        public class Tower
        {
            [JsonPropertyName("first")]
            public bool First { get; set; }

            [JsonPropertyName("kills")]
            public int Kills { get; set; }
        }


    }

}
