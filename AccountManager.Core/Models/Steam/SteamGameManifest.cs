
namespace AccountManager.Core.Models.Steam
{
    public sealed class SteamGameManifest
    {
        public string? AppId { get; set; }
        public string? Name { get; set; }
        public string? LastOwner { get; set; }
    }

    public sealed class SteamGameManifestWrapper
    {
        public SteamGameManifest AppState { get; set; } = new();
    }
}
