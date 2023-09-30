using Microsoft.AspNetCore.Components;
using AccountManager.Core.Enums;
using AccountManager.Core.Models;
using Blazorise.Charts;
using AccountManager.Core.Attributes;

namespace AccountManager.Blazor.Components.AccountListTile.TileContent.Pages.Valorant
{
    [AccountTilePage(AccountType.Riot, 3)]
    [AccountTilePage(AccountType.Valorant, 3)]
    public partial class ValorantRankedWinsPage
    {
		[CascadingParameter]
		public Account? Account { get; set; }
        [CascadingParameter(Name = "RegisterTileDataRefresh")]
        Action<Action> RegisterTileDataRefresh { get; set; } = delegate { };
        private Account? _account = null;
        LineChart<CoordinatePair>? lineChart;
        LineChartOptions lineChartOptions = new()
        {
            MaintainAspectRatio = false,
            Scales = new()
            {
                X = new()
                {
                    Ticks = new()
                    {
                        Font = new()
                        { Family = "Roboto", Size = 10 },
                    },
                    Title = new()
                    {
                        Font = new()
                        { Family = "Roboto", Size = 10 },
                    },
                    Time = new()
                    { Unit = "day", },
                    Type = "timeseries",
                },
                Y = new()
                {
                    Ticks = new()
                    {
                        Font = new()
                        { Family = "Roboto", Size = 10 }
                    },
                    Title = new()
                    {
                        Font = new()
                        { Family = "Roboto", Size = 10 }
                    }
                },
            },
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
                    Position = "left",
                    Text = "Ranked Wins"
                }
            }
        };
        protected override async Task OnInitializedAsync()
        {
            RegisterTileDataRefresh(() => Task.Run(UpdateValorantWins));
            await base.OnInitializedAsync();
        }

        private async Task UpdateValorantWins()
        {
            if (Account is null)
                return;

            try
            {
                displayGraph = await _valorantGraphService.GetRankedWinsLineGraph(Account);
            }
            catch
            {
                _alertService.AddErrorAlert($"Unable to display ranked win graph account {Account.Name}.");
            }

            await HandleRedraw();
        }

        async Task HandleRedraw()
        {
            lineChart?.Clear();
            if (lineChart is null)
                return;
            var datasets = displayGraph;
            if (datasets?.Data is null)
                return;
            datasets.Data = datasets.Data.OrderBy((data) => string.IsNullOrEmpty(data.ColorHex) ? 1 : 0).ToList();
            var chartDatasets = datasets.Data.Select((dataset) => new LineChartDataset<CoordinatePair>
            {
                Label = dataset.Label,
                Data = dataset.Data,
                BackgroundColor = !string.IsNullOrEmpty(dataset?.ColorHex) ? dataset.ColorHex + "90" // Add an alpha value to the end of the hex color to make it slightly translucent
                    : backgroundColors,
                BorderColor = dataset?.ColorHex != null ? new List<string> { dataset.ColorHex } : borderColors,
                Fill = false,
                PointRadius = 3,
                Hidden = dataset?.Hidden ?? false,
                PointBorderColor = borderColors,
                SpanGaps = false
            });
            await lineChart.AddDatasetsAndUpdate(chartDatasets.ToArray());
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
                await HandleRedraw();
        }

        protected override async Task OnParametersSetAsync()
        {
            if (_account != Account && Account is not null)
            {
                _account = Account;

                await UpdateValorantWins();
            }
        }

        LineGraph? displayGraph;
        List<string> backgroundColors = new List<string> { ChartColor.FromRgba(255, 99, 132, 0.2f), ChartColor.FromRgba(54, 162, 235, 0.2f), ChartColor.FromRgba(255, 206, 86, 0.2f), ChartColor.FromRgba(75, 192, 192, 0.2f), ChartColor.FromRgba(153, 102, 255, 0.2f), ChartColor.FromRgba(255, 159, 64, 0.2f) };
        List<string> borderColors = new List<string> { ChartColor.FromRgba(255, 99, 132, 1f), ChartColor.FromRgba(54, 162, 235, 1f), ChartColor.FromRgba(255, 206, 86, 1f), ChartColor.FromRgba(75, 192, 192, 1f), ChartColor.FromRgba(153, 102, 255, 1f), ChartColor.FromRgba(255, 159, 64, 1f) };
    }
}