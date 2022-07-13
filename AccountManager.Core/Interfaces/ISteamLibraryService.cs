using AccountManager.Core.Models;
using AccountManager.Core.Models.Steam;

namespace AccountManager.Core.Interfaces
{
    public interface ISteamLibraryService
    {
        List<SteamGameManifest> GetGameManifests();
        LibraryFoldersWrapper GetLibraryFolders();
        bool TryGetSteamDirectory(out string steamDirectory);
        bool TryGetUserId(Account account, out string userId);
    }
}