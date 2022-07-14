namespace AccountManager.Infrastructure.Services.FileSystem
{
    public interface ISteamFileSystemService
    {
        List<string[]> GetInstalledGamesManifest(string libraryPath);
        bool TryGetSteamDirectory(out string steamDrive);
    }
}