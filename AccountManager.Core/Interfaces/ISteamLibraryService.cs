using AccountManager.Core.Models;
using AccountManager.Core.Models.Steam;

namespace AccountManager.Core.Interfaces
{
    public interface ISteamLibraryService
    {
        bool TryGetGameManifests(out List<SteamGameManifest> gameManifests);
        bool TryGetLibraryFolders(out LibraryFoldersWrapper? libraryFolders);
        bool TryGetSteamDirectory(out string steamDirectory);
        bool TryGetUserId(Account account, out string userId);
    }
}