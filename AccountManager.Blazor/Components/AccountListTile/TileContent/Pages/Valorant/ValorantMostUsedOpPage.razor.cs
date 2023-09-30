using Microsoft.AspNetCore.Components;
using AccountManager.Core.Models;
using Blazorise.Charts;
using AccountManager.Core.Attributes;

namespace AccountManager.Blazor.Components.AccountListTile.TileContent.Pages.Valorant
{
    [AccountTilePage(Core.Enums.AccountType.Valorant, 4)]
    public partial class ValorantMostUsedOpPage
    {
		[CascadingParameter]
		public Account? Account { get; set; }
        [CascadingParameter(Name = "RegisterTileDataRefresh")]
        Action<Action> RegisterTileDataRefresh { get; set; } = delegate { };
        private Account? _account = null;

        private PieChart<PieChartData>? pieChart;
        private readonly PieChartOptions pieChartOptions = new()
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
                    Text = "Recently Used Operators"
                }
            }
        };
        protected override async Task OnInitializedAsync()
        {
            RegisterTileDataRefresh(() => Task.Run(UpdateRecentlyUsedOperators));
            await base.OnInitializedAsync();
        }

        private async Task UpdateRecentlyUsedOperators()
        {
            if (Account is null)
                return;

            try
            {
                displayGraph = await _valorantGraphService.GetRecentlyUsedOperatorsPieChartAsync(Account);
            }
            catch
            {
                _alertService.AddErrorAlert($"Unable to display Recently used operators for account {Account.Name}.");
            }

            await HandleRedraw();
        }

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
            if (_account != Account && Account is not null)
            {
                _account = Account;

                await UpdateRecentlyUsedOperators();
            }
        }

        PieChart? displayGraph;
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