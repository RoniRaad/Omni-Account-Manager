using AccountManager.Core.Models.EpicGames;

namespace AccountManager.Infrastructure.Services
{
    public interface IEpicGamesLibraryService
    {
        bool TryGetInstalledGames(out List<EpicGamesInstalledGame> installedGames);
    }
}