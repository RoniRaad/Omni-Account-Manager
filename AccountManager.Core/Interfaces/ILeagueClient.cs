using AccountManager.Core.Models;
using AccountManager.Core.Models.RiotGames.League;
using AccountManager.Core.Models.RiotGames.League.Responses;
using AccountManager.Core.Models.RiotGames.TeamFightTactics.Responses;

namespace AccountManager.Core.Interfaces
{
    public interface ILeagueClient
    {
        Task<List<LeagueQueueMapResponse>?> GetLeagueQueueMappings();
        Task<Rank> GetSummonerRankByPuuidAsync(Account account);
        Task<Rank> GetTFTRankByPuuidAsync(Account account);
        Task<UserChampSelectHistory?> GetUserChampSelectHistory(Account account);
        Task<MatchHistory?> GetUserLeagueMatchHistory(Account account);
        Task<TeamFightTacticsMatchHistory?> GetUserTeamFightTacticsMatchHistory(Account account);
    }
}