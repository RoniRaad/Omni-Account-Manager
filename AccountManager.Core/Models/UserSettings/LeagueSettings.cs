namespace AccountManager.Core.Models.UserSettings
{
    public class LeagueSettings
    {
        public LeagueSettings() { }

        public bool UseAccountCredentials { get; set; } = true;
        public Guid? AccountToUseCredentials { get; set; }
    }
}
