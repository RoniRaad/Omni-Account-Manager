using AccountManager.Core.Models;

namespace AccountManager.Core.Interfaces
{
    public interface IPlatformService
    {
        Task Login(Account account);
        Task<(bool, string)> TryFetchId(Account account);
        Task<(bool, Rank)> TryFetchRank(Account account);
        Task<(bool, Graphs)> TryFetchRankedGraphs(Account account);
    }
}