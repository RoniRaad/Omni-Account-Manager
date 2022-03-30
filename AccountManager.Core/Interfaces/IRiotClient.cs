using AccountManager.Core.Models;
using AccountManager.Core.Models.RiotGames;
using AccountManager.Core.Models.RiotGames.League.Requests;
using System.Net;

namespace AccountManager.Core.Interfaces
{
    public interface IRiotClient
    {
        Task<string> GetEntitlementToken(string token);
        Task<string?> GetPuuId(string username, string password);
        Task<RiotAuthResponse> GetRiotClientInitialCookies(InitialAuthTokenRequest request, Account account);
        Task<string?> GetToken(Account account);
        Task<Rank> GetValorantRank(Account account);
        Task<RiotAuthResponse> RiotAuthenticate(Account account, RiotAuthCookies initialCookies);
    }
}