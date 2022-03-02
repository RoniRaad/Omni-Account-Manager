using AccountManager.Core.Models;

namespace AccountManager.Infrastructure.Clients
{
    public interface IRiotClient
    {
        Task GetAuth(string username, string password);
        Task<string> GetEntitlementToken(string token);
        Task<string> GetPuuId(string username, string password);
        Task<string> GetToken(string username, string pass);
        Task<string> GetValorantRank(Account account);
    }
}