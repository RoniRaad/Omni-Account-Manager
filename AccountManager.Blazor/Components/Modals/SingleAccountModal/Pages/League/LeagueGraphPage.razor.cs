using Microsoft.AspNetCore.Components;
using AccountManager.Core.Models;
using AccountManager.Core.Attributes;

namespace AccountManager.Blazor.Components.Modals.SingleAccountModal.Pages.League
{
    [SingleAccountPage("Data", Core.Enums.AccountType.League)]
    public partial class LeagueGraphPage
    {
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
            List<Task> graphTasks;

            if (Account is null)
                return;

            try
            {
                graphTasks = new()
                {
                    Task.Run(async () => rankedWinsGraph = await _graphService.GetRankedWinsGraph(Account)),
                    Task.Run(async () => rankedChampSelectPieChart = await _graphService.GetRankedChampSelectPieChart(Account)),
                    Task.Run(async () => rankedWinrateByChamp = await _graphService.GetRankedWinrateByChampBarChartAsync(Account)),
                    Task.Run(async () => rankedCsRateByChamp = await _graphService.GetRankedCsRateByChampBarChartAsync(Account))
                };

                await Task.WhenAll(graphTasks);
            }
            catch
            {
                _alertService.AddErrorAlert("Unable to get graph information for league account.");
            }

            if (rankedWinrateByChamp is not null)
                rankedWinrateByChamp.Type = "percent";
        }
    }
}