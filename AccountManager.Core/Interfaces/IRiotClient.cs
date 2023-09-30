using AccountManager.Core.Models;
using AccountManager.Core.Models.RiotGames;

namespace AccountManager.Core.Interfaces
{
    public interface IRiotClient
    {
        Task<string?> GetPuuId(Account account);
        Task<string?> GetExpectedClientVersion();
        Task<RegionInfo> GetValorantRegionInfo(Account account);
    }
}