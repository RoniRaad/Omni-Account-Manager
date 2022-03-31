using AccountManager.Core.Models;
using AccountManager.Core.Models.RiotGames;
using AccountManager.Core.Models.RiotGames.Requests;

namespace AccountManager.Core.Interfaces
{
    public interface IRiotClient
    {
        Task<string> GetEntitlementToken(string token);
        Task<string?> GetPuuId(string username, string password);
        Task<RiotAuthResponse> GetRiotClientInitialCookies(InitialAuthTokenRequest request, Account account);
        Task<string?> GetValorantToken(Account account);
        Task<Rank> GetValorantRank(Account account);
        Task<RiotAuthResponse> RiotAuthenticate(Account account, RiotAuthCookies initialCookies);
    }
}