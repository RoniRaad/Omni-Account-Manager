using AccountManager.Core.Models;

namespace AccountManager.Core.Interfaces
{
    public interface ILeagueClient
    {
        Task<Rank> GetRankByPuuidAsync(Account account);
        Task<string> GetRankByUsernameAsync(string username);
    }
}