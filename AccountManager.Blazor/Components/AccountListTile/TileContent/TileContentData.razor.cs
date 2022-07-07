using Microsoft.AspNetCore.Components;
using AccountManager.Blazor.Components.AccountListTile.TileContent.Pages;
using AccountManager.Blazor.Components.AccountListTile.TileContent.Pages.League;
using AccountManager.Blazor.Components.AccountListTile.TileContent.Pages.Valorant;
using AccountManager.Core.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Reflection;
using AccountManager.Blazor.Components.AccountListTile.TileContent.Pages.TeamFightTactics;

namespace AccountManager.Blazor.Components.AccountListTile.TileContent
{
    public partial class TileContentData
    {
        [Parameter]
        public Account Account { get; set; } = new();

        private List<Type> pages = new();
        private Dictionary<string, object> pageParams = new();
        private int activePage = 0;
        protected override void OnParametersSet()
        {
            pageParams["Account"] = Account;
            var currentPages = pages;
            pages = _cache?.GetOrCreate($"{Account.AccountType}.pages", cacheEntry =>
            {
                Type currentPageType;
                switch (Account.AccountType)
                {
                    case Core.Enums.AccountType.League:
                        currentPageType = typeof(ILeaguePage);
                        break;
                    case Core.Enums.AccountType.Valorant:
                        currentPageType = typeof(IValorantPage);
                        break;
                    case Core.Enums.AccountType.TeamFightTactics:
                        currentPageType = typeof(ITeamFightTacticsPage);
                        break;
                    default:
                        currentPageType = typeof(IDefaultPage);
                        break;
                }

                var releventPages = AppDomain.CurrentDomain?.GetAssemblies()?.SelectMany(x => x.GetTypes()).Where(t => t.GetInterfaces().Contains(currentPageType))?.ToList();
                releventPages = releventPages?.OrderBy((item) => item?.GetField("OrderNumber", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy)?.GetValue(null))?.ToList();
                return releventPages;
            }) ?? new();
            if (currentPages != pages)
                activePage = 0;
        }

        protected override void OnInitialized()
        {
        }

        private string getPageButtonClass(int pageNum)
        {
            if (activePage == pageNum)
                return "active";
            return "";
        }

        private void incrementPage()
        {
            activePage++;
            if (activePage >= pages?.Count)
                activePage = 0;
        }

        private void decrementPage()
        {
            activePage--;
            if (activePage < 0)
                activePage = pages.Count - 1;
        }

        private void setPage(int pageNum)
        {
            activePage = pageNum;
        }
    }
}