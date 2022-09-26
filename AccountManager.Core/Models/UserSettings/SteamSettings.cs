namespace AccountManager.Core.Models.UserSettings
{
    public sealed class SteamSettings
    {
        public SteamSettings() { }
        public bool OnlyShowOwnedSteamGames { get; set; } = true;
    }
}
