using AccountManager.Core.Enums;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Models.RiotGames.Valorant.Responses;
using AccountManager.Core.Services.GraphServices;
using AccountManager.Infrastructure.Clients;
using Blazorise.Charts;
using System.Reflection;
using System.Security.Principal;

namespace AccountManager.Blazor.State
{
    public class AccountListState
    {
        private readonly IAppState _appState;
        private readonly ILeagueGraphService _leagueGraphService;

        public AccountListState(IAppState appState, ILeagueGraphService leagueGraphService, IGenericFactory<AccountType, IAccountListItem> _listItemFactory)
        {
            _appState = appState;
            _leagueGraphService = leagueGraphService;
            _appState.Accounts.ForEach((acc) =>
            {
                try
                {
                    if (!string.IsNullOrEmpty(acc.Id))
                    {
                        var newAccount = _listItemFactory.CreateImplementation(acc.AccountType);
                        Task.Run(async () => await newAccount.SetAccount(acc));
                        Accounts.Add(newAccount);
                    }

                }
                catch
                {

                }
            });
        }

        public List<IAccountListItem> Accounts { get; set; } = new();
    }

    [System.AttributeUsage(System.AttributeTargets.Class)]

    public class AccountListItemAttribute : System.Attribute
    {
        public AccountType Type { get; set; }

        public AccountListItemAttribute(AccountType type)
        {
            this.Type = type;
        }
    }

    public interface IAccountListItem
    {
        event EventHandler DataRefreshed;
        public Account? Account { get; }
        public Task SetAccount(Account account);
        public Task RefreshData();
    }

    [AccountListItem(AccountType.League)]
    public class LeagueAccountListItem : IAccountListItem
    {
        private readonly ILeagueGraphService _leagueGraphService;
        private Account? _account;

        public event EventHandler DataRefreshed = delegate { };

        public Account? Account { 
            get
            {
                return _account;
            }
        }

        public LeagueAccountListItem(ILeagueGraphService leagueGraphService)
        {
            _leagueGraphService = leagueGraphService;
        }

        public LeaguePageData PageData { get; set; } = new();

        public async Task SetAccount(Account account)
        {
            _account = account;
            await RefreshData();
        }

        public async Task RefreshData()
        {
            if (_account is null)
                return;
            var rankedCsRateChartTask = _leagueGraphService.GetRankedCsRateByChampBarChartAsync(_account);
            var rankedChampSelectPieChartTask = _leagueGraphService.GetRankedChampSelectPieChart(_account);
            var rankedWinrateChartTask = _leagueGraphService.GetRankedWinrateByChampBarChartAsync(_account);
            var rankedWinsChartTask = _leagueGraphService.GetRankedWinsGraph(_account);

            await Task.WhenAll(rankedCsRateChartTask, rankedChampSelectPieChartTask, rankedWinrateChartTask, rankedWinsChartTask);

            PageData.CsPerMinute.Chart = rankedCsRateChartTask.Result;
            PageData.MostUsedChamp.Chart = rankedChampSelectPieChartTask.Result;
            PageData.RecentWinrate.Chart = rankedWinrateChartTask.Result;
            PageData.Wins.Chart = rankedWinsChartTask.Result;

            DataRefreshed.Invoke(this, new());
        }
    }

    public class ValorantAccountListItem : IAccountListItem
    {
        private readonly IValorantGraphService _valorantGraphService;
        private readonly IValorantClient _valorantClient;
        private Account? _account;

        public event EventHandler DataRefreshed = delegate { };

        public Account? Account
        {
            get
            {
                return _account;
            }
        }
        public ValorantAccountListItem(IValorantGraphService valorantGraphService, IValorantClient valorantClient)
        {
            _valorantGraphService = valorantGraphService;
            _valorantClient = valorantClient;
        }
        public ValorantPageData PageData { get; set; } = new();

        public async Task RefreshData()
        {
            PageData.AverageAcs.Chart = await _valorantGraphService.GetRankedACS(Account);
            PageData.MostUsedOp.Chart = await _valorantGraphService.GetRecentlyUsedOperatorsPieChartAsync(Account);
            PageData.RRChange.Chart = await _valorantGraphService.GetRankedRRChangeLineGraph(Account);
            PageData.StoreFrontSkins = await _valorantClient.GetValorantShopDeals(Account);

            DataRefreshed.Invoke(this, new());
        }

        public async Task SetAccount(Account account)
        {
            _account = account;
            await RefreshData();
        }
    }

    public class TeamFightTacticsAccountListItem : IAccountListItem
    {
        private readonly ITeamFightTacticsGraphService _teamFightTacticsGraphService;
        private Account? _account;

        public event EventHandler DataRefreshed = delegate { };

        public Account? Account
        {
            get
            {
                return _account;
            }
        }
        public TeamFightTacticsAccountListItem(ITeamFightTacticsGraphService teamFightTacticsGraphService)
        {
            _teamFightTacticsGraphService = teamFightTacticsGraphService;
        }
        public TeamFightTacticsPageData PageData { get; set; } = new();

        public async Task RefreshData()
        {
            PageData.Wins.Chart = await _teamFightTacticsGraphService.GetRankedPlacementOffset(_account);

            DataRefreshed.Invoke(this, new());
        }

        public async Task SetAccount(Account account)
        {
            _account = account;
            await RefreshData();
        }
    }

    public class LeaguePageData
    {
        public BarChartPageModel CsPerMinute { get; set; } = new();
        public PieGraphPageModel MostUsedChamp { get; set; } = new();
        public BarChartPageModel RecentWinrate { get; set; } = new();
        public LineGraphPageModel Wins { get; set; } = new();
    }

    public class TeamFightTacticsPageData
    {
        public LineGraphPageModel Wins { get; set; } = new();
    }

    public class ValorantPageData
    {
        public BarChartPageModel AverageAcs { get; set; } = new();
        public PieGraphPageModel MostUsedOp { get; set; } = new();
        public LineGraphPageModel RankedWins { get; set; } = new();
        public LineGraphPageModel RRChange { get; set; } = new();
        public List<ValorantSkinLevelResponse> StoreFrontSkins { get; set; } = new();
    }

    public class BarChartPageModel
    {
        public BarChart? Chart { get; set; }
        private readonly BarChartOptions BarChartOptions = new()
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
        public string? Title { 
            get { 
                return BarChartOptions.Plugins.Title.Text; 
            } 
            set { 
                BarChartOptions.Plugins.Title.Text = value; 
            } 
        }
    }

    public class LineGraphPageModel
    {
        public LineGraph? Chart { get; set; }
        private readonly LineChartOptions LineChartOptions = new()
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
                    Position = "left"
                }
            }
        };

        public string? Title
        {
            get
            {
                return LineChartOptions.Plugins.Title.Text;
            }
            set
            {
                LineChartOptions.Plugins.Title.Text = value;
            }
        }
    }

    public class PieGraphPageModel
    {
        public PieChart? Chart { get; set; }
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

        public string? Title
        {
            get
            {
                return PieChartOptions.Plugins.Title.Text;
            }
            set
            {
                PieChartOptions.Plugins.Title.Text = value;
            }
        }
    }
}

