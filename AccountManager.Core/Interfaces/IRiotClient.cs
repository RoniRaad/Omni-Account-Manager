using AccountManager.Core.Models;

namespace AccountManager.Core.Interfaces
{
    public interface IRiotClient
    {
        Task<string> GetEntitlementToken(string token);
        Task<string> GetPuuId(string username, string password);
        Task<string> GetToken(Account account);
        Task<Rank> GetValorantRank(Account account);
    }
}