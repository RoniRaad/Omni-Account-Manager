using AccountManager.Core.Models;
using AccountManager.Core.Models.RiotGames;
using AccountManager.Core.Models.RiotGames.Requests;

namespace AccountManager.Infrastructure.Clients
{
    public interface IRiotTokenClient
    {
        Task<RiotAuthTokensResponse> GetRiotTokens(RiotTokenRequest request, Account account);
        Task<string?> GetEntitlementToken(string accessToken);
        Task<string?> GetExpectedClientVersion();
    }
}