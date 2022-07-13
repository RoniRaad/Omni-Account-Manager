
namespace AccountManager.Core.Models.Steam
{
    public class SteamGameManifest
    {
        public string? AppId { get; set; }
        public string? Name { get; set; }
        public string? LastOwner { get; set; }
    }

    public class SteamGameManifestWrapper
    {
        public SteamGameManifest AppState { get; set; } = new();
    }
}
