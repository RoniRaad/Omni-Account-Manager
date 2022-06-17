using AccountManager.Core.Models;

namespace AccountManager.Core.Interfaces
{
    public interface ILeagueGraphService
    {
        Task<PieChart> GetRankedChampSelectPieChart(Account account);
        Task<BarChart> GetRankedCsRateByChampBarChartAsync(Account account);
        Task<BarChart> GetRankedWinrateByChampBarChartAsync(Account account);
        Task<LineGraph> GetRankedWinsGraph(Account account);
    }
}