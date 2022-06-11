using AccountManager.Core.Models;

namespace AccountManager.Core.Interfaces
{
    public interface IValorantGraphService
    {
        Task<LineGraph> GetRankedRRChangeLineGraph(Account account);
        Task<LineGraph> GetRankedWinsLineGraph(Account account);
        Task<PieChart> GetRecentlyUsedOperatorsPieChartAsync(Account account);
    }
}