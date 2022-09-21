using AccountManager.Core.Models;

namespace AccountManager.Core.Interfaces
{
    public interface IEpicGamesExternalAuthService
    {
        Task<EpicGamesLoginInfo?> TryGetEpicGamesAccessTokens(string username, string password);
    }
}