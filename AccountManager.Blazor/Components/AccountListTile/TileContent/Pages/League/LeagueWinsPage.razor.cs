using Microsoft.AspNetCore.Components;
using AccountManager.Core.Models;
using Blazorise.Charts;
using AccountManager.Core.Attributes;
using AccountManager.Blazor.State;

namespace AccountManager.Blazor.Components.AccountListTile.TileContent.Pages.League
{
    [AccountTilePage(Core.Enums.AccountType.League, 0)]
    public partial class LeagueWinsPage
    {
        private Account _account = new();
        [Parameter]
        public Account Account { get; set; } = new();
        [CascadingParameter]
        public IAccountListItem? AccountListItem { get; set; }
        private LeagueAccountListItem? _accountListItem;
        LineChart<CoordinatePair>? lineChart;

        private readonly LineChartOptions lineChartOptions = new()
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
                    Text = "Wins"
                }
            }
        };

        async Task HandleRedraw()
        {
            lineChart?.Clear();
            if (lineChart is null || _accountListItem is null)
                return;

            var datasets = _accountListItem?.PageData?.Wins?.Chart;
            if (datasets is null)
                return;

            datasets.Data = datasets.Data.OrderBy((data) => string.IsNullOrEmpty(data.ColorHex) ? 1 : 0).ToList();
            var chartDatasets = datasets.Data.Select((dataset) => new LineChartDataset<CoordinatePair>
            {
                Label = dataset.Label,
                Data = dataset.Data,
                BackgroundColor = !string.IsNullOrEmpty(dataset?.ColorHex) ? dataset.ColorHex + "90" : backgroundColors,  // Add an alpha value to the end of the hex color to make it slightly translucent
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
            {
                await HandleRedraw();
                if (_accountListItem is not null)
                    _accountListItem.DataRefreshed += async (caller, e) =>
                    {
                        await HandleRedraw();
                        await InvokeAsync(() => StateHasChanged());
                    };
                }
        }

        protected override async Task OnParametersSetAsync()
        {
            if (AccountListItem is not null)
                _accountListItem = AccountListItem as LeagueAccountListItem;

            if (_account != Account)
            {
                _account = Account;

                await HandleRedraw();
                await InvokeAsync(() => StateHasChanged());
            }
        }

        private readonly List<string> backgroundColors = new List<string> { ChartColor.FromRgba(255, 99, 132, 0.2f), ChartColor.FromRgba(54, 162, 235, 0.2f), ChartColor.FromRgba(255, 206, 86, 0.2f), ChartColor.FromRgba(75, 192, 192, 0.2f), ChartColor.FromRgba(153, 102, 255, 0.2f), ChartColor.FromRgba(255, 159, 64, 0.2f) };
        private readonly List<string> borderColors = new List<string> { ChartColor.FromRgba(255, 99, 132, 1f), ChartColor.FromRgba(54, 162, 235, 1f), ChartColor.FromRgba(255, 206, 86, 1f), ChartColor.FromRgba(75, 192, 192, 1f), ChartColor.FromRgba(153, 102, 255, 1f), ChartColor.FromRgba(255, 159, 64, 1f) };
    }
}