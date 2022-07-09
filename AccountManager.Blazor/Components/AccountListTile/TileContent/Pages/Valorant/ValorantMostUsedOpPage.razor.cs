using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using AccountManager.Blazor.Shared;
using AccountManager.Blazor;
using Plk.Blazor.DragDrop;
using AccountManager.Core.Enums;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Services;
using Blazorise.Charts;
using System.Security.Principal;

namespace AccountManager.Blazor.Components.AccountListTile.TileContent.Pages.Valorant
{
    public partial class ValorantMostUsedOpPage
    {
        public static int OrderNumber = 4;

        [Parameter]
        public Account Account { get; set; } = new();

        private Account _account = new();

        PieChart<PieChartData>? pieChart;
        PieChartOptions pieChartOptions = new()
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

        protected override async Task OnInitializedAsync()
        {
            _account = Account;
        }

        protected override async Task OnAfterRenderAsync(bool first)
        {
            if (first)
            {
                displayGraph = await _valorantGraphService.GetRecentlyUsedOperatorsPieChartAsync(Account);
                await HandleRedraw();

                await InvokeAsync(() => StateHasChanged());
            }
        }

        protected override async Task OnParametersSetAsync()
        {
            if (_account != Account)
            {
                _account = Account;

                displayGraph = await _valorantGraphService.GetRecentlyUsedOperatorsPieChartAsync(Account);
                await HandleRedraw();
            }
        }

        PieChart? displayGraph;
        List<string> backgroundColors = new List<string> { ChartColor.FromRgba(255, 99, 132, 0.2f), ChartColor.FromRgba(54, 162, 235, 0.2f), ChartColor.FromRgba(255, 206, 86, 0.2f), ChartColor.FromRgba(75, 192, 192, 0.2f), ChartColor.FromRgba(153, 102, 255, 0.2f), ChartColor.FromRgba(255, 159, 64, 0.2f) };
        List<string> borderColors = new List<string> { ChartColor.FromRgba(255, 99, 132, 1f), ChartColor.FromRgba(54, 162, 235, 1f), ChartColor.FromRgba(255, 206, 86, 1f), ChartColor.FromRgba(75, 192, 192, 1f), ChartColor.FromRgba(153, 102, 255, 1f), ChartColor.FromRgba(255, 159, 64, 1f) };
    }
}