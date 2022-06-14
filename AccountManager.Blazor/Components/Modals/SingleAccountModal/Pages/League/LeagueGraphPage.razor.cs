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
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;

namespace AccountManager.Blazor.Components.Modals.SingleAccountModal.Pages.League
{
    public partial class LeagueGraphPage
    {
        public static string Title = "Data";
        [Parameter]
        public string navigationTitle { get; set; }
        [Parameter, EditorRequired]
        public Account Account { get; set; }

        [Parameter, EditorRequired]
        public Action IncrementPage { get; set; }

        [Parameter, EditorRequired]
        public Action DecrementPage { get; set; }

        LineGraph rankedWinsGraph;
        PieChart rankedChampSelectPieChart;
        BarChart rankedWInrateByChamp;
        BarChart rankedCsRateByChamp;
        protected override async Task OnParametersSetAsync()
        {
            navigationTitle = Title;
            if (Account is null)
                return;
            rankedWinsGraph = await _graphService.GetRankedWinsGraph(Account);
            rankedChampSelectPieChart = await _graphService.GetRankedChampSelectPieChart(Account);
            rankedWInrateByChamp = await _graphService.GetRankedWinrateByChampBarChartAsync(Account);
            rankedCsRateByChamp = await _graphService.GetRankedCsRateByChampBarChartAsync(Account);
            rankedWInrateByChamp.Type = "percent";
        }
    }
}