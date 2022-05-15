using AccountManager.Core.Models;
using AccountManager.Core.Models.RiotGames.League.Requests;

namespace AccountManager.Core.Interfaces
{
    public interface ILeagueClient
    {
        Task<Rank> GetSummonerRankByPuuidAsync(Account account);
        Task<Rank> GetTFTRankByPuuidAsync(Account account);
        Task<MatchHistoryRequest?> GetUserMatchHistory(Account account, int startIndex, int endIndex);
    }
}