using AccountManager.Core.Models;

namespace AccountManager.Core.Interfaces
{
    public interface IPlatformService
    {
        Task Login(Account account);
        Task<string> TryFetchId(Account account);
        Task<string> TryFetchRank(Account account);
    }
}