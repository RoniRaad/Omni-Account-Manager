using AccountManager.Core.Models;

namespace AccountManager.Core.Interfaces
{
    public interface ILeagueClient
    {
        Task<Rank> GetSummonerRankByPuuidAsync(Account account);
        Task<Rank> GetTFTRankByPuuidAsync(Account account);
    }
}