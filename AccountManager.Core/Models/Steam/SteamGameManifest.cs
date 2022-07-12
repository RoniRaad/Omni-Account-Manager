
namespace AccountManager.Core.Models.Steam
{
    public class SteamGameManifest
    {
        public string? appid { get; set; }
        public string? name { get; set; }
        public string? LastOwner { get; set; }
    }

    public class SteamGameManifestWrapper
    {
        public SteamGameManifest AppState { get; set; }
    }
}
