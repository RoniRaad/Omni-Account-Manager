using Microsoft.AspNetCore.Components;
using AccountManager.Core.Models;
using Blazorise.Charts;
using AccountManager.Core.Attributes;

namespace AccountManager.Blazor.Components.AccountListTile.TileContent.Pages.League
{
    [AccountTilePage(Core.Enums.AccountType.League, 3)]
    public partial class LeagueMostUsedChampPage
    {
        [Parameter]
        public Account Account { get; set; } = new();
        PieChart? displayGraph;
        private Account _account = new();
        private PieChart<PieChartData>? pieChart;
        private readonly PieChartOptions PieChartOptions = new()
        {
            MaintainAspectRatio = false,
            Plugins = new()
            {
                Legend = new()
                {
                    Labels = new()
                    {
                        Font = new()
                        { Family = "Roboto", Size = 10 },
                        BoxHeight = 10,
                        BoxWidth = 16
                    },
                },
                Title = new()
                {
                    Font = new()
                    { Family = "Roboto", Size = 10 },
                    Display = true,
                    Padding = 1,
                    Position = "top",
                    Text = "Recently Used Champs"
                }
            }
        };
        async Task HandleRedraw()
        {
            pieChart?.Clear();
            if (pieChart is null)
                return;
            var datasets = displayGraph;
            if (datasets?.Data is null)
                return;
            var chartDatasets = new PieChartDataset<PieChartData> { Data = datasets?.Data?.ToList(), BackgroundColor = backgroundColors, BorderColor = borderColors, };
            await pieChart.AddLabelsDatasetsAndUpdate(datasets?.Labels, chartDatasets);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
                await HandleRedraw();
        }

        protected override async Task OnParametersSetAsync()
        {
            if (_account != Account)
            {
                _account = Account;

                try
                {
                    displayGraph = await _leagueGraphService.GetRankedChampSelectPieChart(Account);
                }
                catch
                {
                    _alertService.AddErrorAlert($"Unable to display league most used champs for account {Account.Name}");
                }
                await HandleRedraw();
                await InvokeAsync(() => StateHasChanged());

            }
        }

        private readonly List<string> backgroundColors = new()
        {
            ChartColor.FromRgba(255, 99, 132, 0.2f),
            ChartColor.FromRgba(54, 162, 235, 0.2f),
            ChartColor.FromRgba(255, 206, 86, 0.2f),
            ChartColor.FromRgba(75, 192, 192, 0.2f),
            ChartColor.FromRgba(153, 102, 255, 0.2f),
            ChartColor.FromRgba(255, 159, 64, 0.2f)
        };
        private readonly List<string> borderColors = new()
        {
            ChartColor.FromRgba(255, 99, 132, 1f),
            ChartColor.FromRgba(54, 162, 235, 1f),
            ChartColor.FromRgba(255, 206, 86, 1f),
            ChartColor.FromRgba(75, 192, 192, 1f),
            ChartColor.FromRgba(153, 102, 255, 1f),
            ChartColor.FromRgba(255, 159, 64, 1f)
        };
    }
}