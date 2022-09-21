namespace AccountManager.Infrastructure.Services
{
    public interface IEpicGamesLibraryService
    {
        bool TryGetInstalledGames(out List<EpicGamesLibraryService.EpicGamesInstalledGame> installedGames);
    }
}