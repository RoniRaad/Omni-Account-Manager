using Microsoft.AspNetCore.Components;
using AccountManager.Blazor.Components.AccountListTile.TileContent.Pages;
using AccountManager.Blazor.Components.AccountListTile.TileContent.Pages.League;
using AccountManager.Blazor.Components.AccountListTile.TileContent.Pages.Valorant;
using AccountManager.Core.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Reflection;
using AccountManager.Blazor.Components.AccountListTile.TileContent.Pages.TeamFightTactics;
using AccountManager.Core.Static;
using AccountManager.Core.Models.UserSettings;
using AccountManager.Core.Attributes;
using AccountManager.Core.Enums;

namespace AccountManager.Blazor.Components.AccountListTile.TileContent
{
    public partial class TileContentData
    {
        [Parameter]
        public Account Account { get; set; } = new();
        [CascadingParameter, EditorRequired]
        public AccountListItemSettings Settings { get; set; } = new();

        private List<Type> pages = new();
        private Dictionary<string, object> pageParams = new();
        private int activePage = 0;
        private Account _account;


        protected override void OnInitialized()
        {
            _accountItemSettings.OnSettingsSaved += () => InvokeAsync(() => StateHasChanged());
        }

        protected override void OnParametersSet()
        {
            if (_account != Account)
            {
                _account = Account;
                pageParams["Account"] = Account;
                var currentPages = pages;
                pages = _cache?.GetOrCreate($"{nameof(TileContentData)}.{Account.AccountType}.Pages", cacheEntry =>
                {
                    return AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(a => a.GetTypes().Where(t => t.IsDefined(typeof(AccountTilePageAttribute), true)))
                        .ToDictionary((element) =>
                        {
                            return Attribute.GetCustomAttribute(element, typeof(AccountTilePageAttribute)) as AccountTilePageAttribute ?? new AccountTilePageAttribute(0, 0);
                        })
                        .Where((kvp) => kvp.Key?.AccountType == Account.AccountType)
                        .OrderBy((kvp) => kvp.Key?.OrderNumber ?? 0)
                        .Select((kvp) => kvp.Value)
                        .ToList();

                }) ?? new();

                if (currentPages != pages)
                    activePage = 0;
            }
        }


        private string GetPageButtonClass(int pageNum)
        {
            if (activePage == pageNum)
                return "active";
            return "";
        }

        private void IncrementPage()
        {
            activePage++;
            if (activePage >= pages?.Count)
                activePage = 0;
        }

        private void DecrementPage()
        {
            activePage--;
            if (activePage < 0)
                activePage = pages.Count - 1;
        }

        private void SetPage(int pageNum)
        {
            activePage = pageNum;
        }
    }
}