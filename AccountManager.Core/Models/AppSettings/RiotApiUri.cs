namespace AccountManager.Core.Models.AppSettings
{
    public sealed class RiotApiUri
    {
        public string? Auth { get; set; }
        public string? Valorant3rdParty { get; set; }
        public string? ValorantNA { get; set; }
        public string? ValorantAP { get; set; }
        public string? ValorantEU { get; set; }
        public string? Entitlement { get; set; }
        public LeagueApiUri League { get; set; } = new();
        public string? RiotCDN { get; set; }
        public string? RiotGeo { get; set; }
        public string UserAgentTemplate { get; set; } = "";
    }

    public sealed class LeagueApiUri
    {
        public string? LeagueBR1 { get; set; }
        public string? LeagueEUN1 { get; set; }
        public string? LeagueEUW1 { get; set; }
        public string? LeagueJP1 { get; set; }
        public string? LeagueLA1 { get; set; }
        public string? LeagueLA2 { get; set; }
        public string? LeagueNA1 { get; set; }
        public string? LeagueOC1 { get; set; }
        public string? LeagueRU { get; set; }
        public string? LeagueTR1 { get; set; }
        public string? LeagueSG2 { get; set; }
        public string? LeaguePH2 { get; set; }
        public string? LeagueVN2 { get; set; }
        public string? LeagueTW2 { get; set; }
        public string? LeagueTH2 { get; set; }
        public string? LeagueSessionEUW1 { get; set; }
        public string? LeagueSessionEUN1 { get; set; }
        public string? LeagueSessionNA1 { get; set; }
        public string? LeagueSessionLA1 { get; set; }
        public string? LeagueSessionLA2 { get; set; }
        public string? LeagueSessionTR1 { get; set; }
        public string? LeagueSessionRU { get; set; }
        public string? LeagueSessionOC1 { get; set; }
        public string? LeagueSessionBR1 { get; set; }
        public string? LeagueSessionJP1 { get; set; }
        public string? LeagueSessionSG2 { get; set; }
        public string? LeagueSessionPH2 { get; set; }
        public string? LeagueSessionVN2 { get; set; }
        public string? LeagueSessionTW2 { get; set; }
        public string? LeagueSessionTH2 { get; set; }
    }
}
