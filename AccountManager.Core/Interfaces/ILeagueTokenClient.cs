using AccountManager.Core.Models;

namespace AccountManager.Infrastructure.Clients
{
    public interface ILeagueTokenClient
    {
        Task<string> CreateLeagueSession(Account account);
        Task<string> GetLeagueSessionToken(Account account);
        Task<string> GetLocalSessionToken();
        Task<bool> TestLeagueToken(string token);
    }
}