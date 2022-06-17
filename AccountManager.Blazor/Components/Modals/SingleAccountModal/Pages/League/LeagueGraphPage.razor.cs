using Microsoft.AspNetCore.Components;
using AccountManager.Core.Models;

namespace AccountManager.Blazor.Components.Modals.SingleAccountModal.Pages.League
{
    public partial class LeagueGraphPage
    {
        public static string Title = "Data";
        [Parameter]
        public string navigationTitle { get; set; } = string.Empty;
        [Parameter, EditorRequired]
        public Account? Account { get; set; }

        [Parameter, EditorRequired]
        public Action? IncrementPage { get; set; }

        [Parameter, EditorRequired]
        public Action? DecrementPage { get; set; }

        LineGraph? rankedWinsGraph;
        PieChart? rankedChampSelectPieChart;
        BarChart? rankedWinrateByChamp;
        BarChart? rankedCsRateByChamp;
        protected override async Task OnInitializedAsync()
        {
            navigationTitle = Title;
            if (Account is null)
                return;
            rankedWinsGraph = await _graphService.GetRankedWinsGraph(Account);
            rankedChampSelectPieChart = await _graphService.GetRankedChampSelectPieChart(Account);
            rankedWinrateByChamp = await _graphService.GetRankedWinrateByChampBarChartAsync(Account);
            rankedCsRateByChamp = await _graphService.GetRankedCsRateByChampBarChartAsync(Account);
            rankedWinrateByChamp.Type = "percent";
        }
    }
}