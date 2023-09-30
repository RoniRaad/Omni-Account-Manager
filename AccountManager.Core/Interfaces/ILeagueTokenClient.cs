using AccountManager.Core.Models;

namespace AccountManager.Infrastructure.Clients
{
    public interface ILeagueTokenClient
    {
        Task<string> CreateLeagueSession();
        Task<string> GetLeagueSessionToken();
        Task<string> GetLocalSessionToken();
        Task<string> GetUserInfo(Account account);
        Task<bool> TestLeagueToken(string token);
    }
}