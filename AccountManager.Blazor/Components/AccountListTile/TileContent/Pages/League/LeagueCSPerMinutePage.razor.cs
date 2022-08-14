using Microsoft.AspNetCore.Components;
using AccountManager.Core.Models;
using Blazorise.Charts;
using AccountManager.Core.Attributes;

namespace AccountManager.Blazor.Components.AccountListTile.TileContent.Pages.League
{
    [AccountTilePage(Core.Enums.AccountType.League, 2)]
    public partial class LeagueCSPerMinutePage
    {
        [Parameter]
        public Account Account { get; set; } = new();
        BarChart? displayGraph;
        private Account _account = new();
        public static int OrderNumber = 2;
        BarChart<double?>? barChart;
        BarChartOptions barChartOptions = new()
        {
            MaintainAspectRatio = false,
            Plugins = new()
            {
                Legend = new()
                {
                    Display = false,
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
                    Position = "top",
                    Text = "Recent CS Per Minute"
                }
            },
            Scales = new()
            {
                X = new()
                { BeginAtZero = true, },
                Y = new()
                {
                    Ticks = new()
                    { }
                }
            },
        };

        async Task HandleRedraw()
        {
            barChart?.Clear();
            if (barChart is null)
                return;
            var datasets = displayGraph;
            if (datasets?.Data is null)
                return;
            var chartDatasets = new BarChartDataset<double?> { Data = datasets?.Data?.Select((data) => data.Value).ToList(), BackgroundColor = backgroundColors, BorderColor = borderColors, BorderWidth = 1, Label = datasets?.Title, SkipNull = false };
            await barChart.AddLabelsDatasetsAndUpdate(datasets?.Labels, chartDatasets);
        }

        protected override async Task OnAfterRenderAsync(bool first)
        {
            if (first)
                await HandleRedraw();
        }

        protected override async Task OnParametersSetAsync()
        {
            if (_account != Account)
            {
                _account = Account;

                try
                {
                    displayGraph = await _leagueGraphService.GetRankedCsRateByChampBarChartAsync(Account);
                }
                catch
                {
                    _alertService.AddErrorAlert($"Unable to display league champ cs rate for account {Account.Id}");
                }
                await HandleRedraw();
                await InvokeAsync(() => StateHasChanged());

            }
        }

        List<string> backgroundColors = new List<string> { ChartColor.FromRgba(255, 99, 132, 0.2f), ChartColor.FromRgba(54, 162, 235, 0.2f), ChartColor.FromRgba(255, 206, 86, 0.2f), ChartColor.FromRgba(75, 192, 192, 0.2f), ChartColor.FromRgba(153, 102, 255, 0.2f), ChartColor.FromRgba(255, 159, 64, 0.2f) };
        List<string> borderColors = new List<string> { ChartColor.FromRgba(255, 99, 132, 1f), ChartColor.FromRgba(54, 162, 235, 1f), ChartColor.FromRgba(255, 206, 86, 1f), ChartColor.FromRgba(75, 192, 192, 1f), ChartColor.FromRgba(153, 102, 255, 1f), ChartColor.FromRgba(255, 159, 64, 1f) };
    }
}