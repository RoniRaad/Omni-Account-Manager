using Microsoft.AspNetCore.Components;
using AccountManager.Core.Models;
using AccountManager.Core.Interfaces;
using Blazorise.Charts;

namespace AccountManager.Blazor.Components
{
    public partial class AccountListItem
    {
        [Parameter, EditorRequired]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Account Account { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [Parameter]
        public bool RenderButtons { get; set; }

        [Parameter, EditorRequired]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Action ReloadList { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        [Parameter, EditorRequired]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Action OpenEditModal { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [Parameter, EditorRequired]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public IAccountService AccountService { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        bool loginDisabled = false;
        string loginBtnStyle => loginDisabled ? "color:darkgrey; pointer-events: none;" : "";
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
                        {
                            Family = "Roboto",
                            Size = 10
                        },


                    },
                    Title = new()
                    {
                        Font = new()
                        {
                            Family = "Roboto",
                            Size = 10
                        }
                    },
                    Time = new()
                    {
                        Unit = "day",
                    },
                    Type = "timeseries",
                },
                Y = new()
                {
                    Ticks = new()
                    {
                        Font = new()
                        {
                            Family = "Roboto",
                            Size = 10
                        }
                    },
                    Title = new()
                    {
                        Font = new()
                        {
                            Family = "Roboto",
                            Size = 10
                        }
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
                        {
                            Family = "Roboto",
                            Size = 10
                        },
                        BoxHeight = 10,
                        BoxWidth = 16
                    },
                }
            }
        };

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await HandleRedraw();
                _state.Notify += async () =>
                {
                    try
                    {
                        await HandleRedraw();
                    }
                    catch
                    {
                        // do nothing
                    }
                };
            }
        }

        async Task Login()
        {
            if (loginDisabled)
                return;

            loginDisabled = true;
            await AccountService.Login(Account);
            loginDisabled = false;
        }
        async Task HandleRedraw()
        {
            lineChart?.Clear();

            if (lineChart is null)
                return;

            var datasets = Account?.Graphs;
            if (datasets is null)
                return;

            datasets = datasets.OrderBy((data) => string.IsNullOrEmpty(data.ColorHex) ? 1 : 0).ToList();
            var chartDatasets = datasets.Select((dataset) => new LineChartDataset<CoordinatePair>
            {
                Label = dataset.Label,
                Data = dataset.Data,
                BackgroundColor = !string.IsNullOrEmpty(dataset?.ColorHex)
                    ? dataset.ColorHex + "90" // Add an alpha value to the end of the hex color to make it slightly translucent
                    : backgroundColors,
                BorderColor = dataset?.ColorHex != null
                    ? new List<string> { dataset.ColorHex }
                    : borderColors,
                Fill = false,
                PointRadius = 3,
                Hidden = dataset?.Hidden ?? false,
                PointBorderColor = borderColors,
            });

            await lineChart.AddDatasetsAndUpdate(chartDatasets.ToArray());
        }

        List<string> backgroundColors = new List<string> { ChartColor.FromRgba(255, 99, 132, 0.2f), ChartColor.FromRgba(54, 162, 235, 0.2f), ChartColor.FromRgba(255, 206, 86, 0.2f), ChartColor.FromRgba(75, 192, 192, 0.2f), ChartColor.FromRgba(153, 102, 255, 0.2f), ChartColor.FromRgba(255, 159, 64, 0.2f) };
        List<string> borderColors = new List<string> { ChartColor.FromRgba(255, 99, 132, 1f), ChartColor.FromRgba(54, 162, 235, 1f), ChartColor.FromRgba(255, 206, 86, 1f), ChartColor.FromRgba(75, 192, 192, 1f), ChartColor.FromRgba(153, 102, 255, 1f), ChartColor.FromRgba(255, 159, 64, 1f) };

        public void Delete()
        {
            AccountService.RemoveAccount(Account);
            ReloadList();
        }
    }
}