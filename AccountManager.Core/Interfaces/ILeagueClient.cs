using AccountManager.Core.Models;
using AccountManager.Core.Models.RiotGames.League.Requests;
using AccountManager.Core.Models.RiotGames.League.Responses;

namespace AccountManager.Core.Interfaces
{
    public interface ILeagueClient
    {
        Task<List<LeagueQueueMapResponse>?> GetLeagueQueueMappings();
        Task<Rank> GetSummonerRankByPuuidAsync(Account account);
        Task<Rank> GetTFTRankByPuuidAsync(Account account);
        Task<UserMatchHistory?> GetUserLeagueMatchHistory(Account account, int startIndex, int endIndex);
        Task<UserMatchHistory?> GetUserTeamFightTacticsMatchHistory(Account account, int startIndex, int endIndex);
    }
}