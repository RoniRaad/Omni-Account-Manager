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
            List<Task> graphTasks;

            if (Account is null)
                return;

            try
            {
                graphTasks = new()
                {
                    Task.Run(async () => rankedRRChangeGraph = await _graphService.GetRankedRRChangeLineGraph(Account)),
                    Task.Run(async () => recentlyUsedOperatorsPieChart = await _graphService.GetRecentlyUsedOperatorsPieChartAsync(Account)),
                    Task.Run(async () => rankedWinGraph = await _graphService.GetRankedWinsLineGraph(Account)),
                    Task.Run(async () => averageACS = await _graphService.GetRankedACS(Account))
                };

                await Task.WhenAll(graphTasks);
            }
            catch
            {
                _alertService.AddErrorAlert("Unable to get graph information for valorant account.");
            }
        }
    }
}