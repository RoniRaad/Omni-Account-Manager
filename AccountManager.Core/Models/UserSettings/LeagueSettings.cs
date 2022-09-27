namespace AccountManager.Core.Models.UserSettings
{
    public sealed class LeagueSettings
    {
        public LeagueSettings() { }

        public bool UseAccountCredentials { get; set; } = true;
        public Guid? AccountToUseCredentials { get; set; }
    }
}
