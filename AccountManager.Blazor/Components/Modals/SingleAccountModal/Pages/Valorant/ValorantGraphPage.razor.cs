using Microsoft.AspNetCore.Components;
using AccountManager.Core.Models;

namespace AccountManager.Blazor.Components.Modals.SingleAccountModal.Pages.Valorant
{
    public partial class ValorantGraphPage
    {
        public static string Title = "Data";
        [Parameter, EditorRequired]
        public Account? Account { get; set; }

        [Parameter, EditorRequired]
        public Action? IncrementPage { get; set; }

        [Parameter, EditorRequired]
        public Action? DecrementPage { get; set; }

        LineGraph? rankedRRChangeGraph;
        PieChart? recentlyUsedOperatorsPieChart;
        LineGraph? rankedWinGraph;
        BarChart? averageACS;
        protected override async Task OnInitializedAsync()
        {
            if (Account is null)
                return;
            rankedRRChangeGraph = await _graphService.GetRankedRRChangeLineGraph(Account);
            recentlyUsedOperatorsPieChart = await _graphService.GetRecentlyUsedOperatorsPieChartAsync(Account);
            rankedWinGraph = await _graphService.GetRankedWinsLineGraph(Account);
            averageACS = await _graphService.GetRankedACS(Account);
        }
    }
}