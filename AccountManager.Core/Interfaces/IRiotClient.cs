using AccountManager.Core.Models;
using AccountManager.Core.Models.RiotGames;
using AccountManager.Core.Models.RiotGames.Requests;
using AccountManager.Core.Models.RiotGames.Valorant.Responses;

namespace AccountManager.Core.Interfaces
{
    public interface IRiotClient
    {
        Task<string?> GetEntitlementToken(string token);
        Task<string?> GetPuuId(string username, string password);
        Task<string?> GetValorantToken(Account account);
        Task<Rank> GetValorantRank(Account account);
        Task<RiotAuthResponse?> RiotAuthenticate(RiotSessionRequest request, Account account);
        Task<ValorantRankedResponse> GetValorantCompetitiveHistory(Account account);
    }
}