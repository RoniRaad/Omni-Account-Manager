using AccountManager.Core.Models;

namespace AccountManager.Core.Interfaces
{
    public interface ITeamFightTacticsGraphService
    {
        Task<LineGraph> GetRankedPlacementOffset(Account account);
    }
}