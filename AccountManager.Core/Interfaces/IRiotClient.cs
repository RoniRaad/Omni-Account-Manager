using AccountManager.Core.Models;
using AccountManager.Core.Models.RiotGames;
using AccountManager.Core.Models.RiotGames.Requests;
using AccountManager.Core.Models.RiotGames.Valorant.Responses;

namespace AccountManager.Core.Interfaces
{
    public interface IRiotClient
    {
        Task<string?> GetEntitlementToken(string token);
        Task<string?> GetPuuId(Account account);
        Task<RiotAuthResponse?> RiotAuthenticate(RiotSessionRequest request, Account account);
        Task<string?> GetExpectedClientVersion();
    }
}