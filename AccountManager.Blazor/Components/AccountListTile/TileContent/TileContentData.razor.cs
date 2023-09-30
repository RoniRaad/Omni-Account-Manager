using Microsoft.AspNetCore.Components;
using AccountManager.Core.Models;
using Microsoft.Extensions.Caching.Memory;
using AccountManager.Core.Models.UserSettings;
using AccountManager.Core.Attributes;
using AccountManager.Core.Static;

namespace AccountManager.Blazor.Components.AccountListTile.TileContent
{
    public partial class TileContentData
    {
        [CascadingParameter]
        public Account? Account { get; set; }
        [CascadingParameter, EditorRequired]
        public AccountListItemSettings Settings { get; set; } = new();

        private List<Type> pages = new();
        private readonly Dictionary<string, object> pageParams = new();
        private int activePage = 0;

        protected override void OnInitialized()
        {
            _accountItemSettings.OnSettingsSaved += () => InvokeAsync(() => StateHasChanged());
        }

        protected override async Task OnInitializedAsync()
        {
            if (Account is null)
                return;

            var currentPages = pages;
            pages = _cache?.GetOrCreate($"{nameof(TileContentData)}.{Account.AccountType}.Pages", cacheEntry =>
            {
                return AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes().Where(t => t.IsDefined(typeof(AccountTilePageAttribute), true)))
                    .SelectMany(type => Attribute.GetCustomAttributes(type, typeof(AccountTilePageAttribute))
                    .Cast<AccountTilePageAttribute>()
                    .Select(attr => new { Attribute = attr, Type = type }))
                    .Where(entry => entry.Attribute.AccountType == Account.AccountType)
                    .OrderBy(entry => entry.Attribute.OrderNumber)
                    .Select(entry => entry.Type)
                    .ToList();

            }) ?? new();

            if (currentPages != pages)
                activePage = await _persistantCache.GetOrCreateAsync($"{nameof(TileContentData)}.{Account.AccountType}.{Account.Id}.CurrentPage", () => Task.FromResult(0));
        }


        private string GetPageButtonClass(int pageNum)
        {
            if (activePage == pageNum)
                return "active";
            return "";
        }

        private async Task IncrementPage()
        {
            if (Account is null)
                return;

            activePage++;
            if (activePage >= pages?.Count)
                activePage = 0;

            await _persistantCache.SetAsync<int>($"{nameof(TileContentData)}.{Account.AccountType}.{Account.Id}.CurrentPage", activePage);
        }

        private async Task DecrementPage()
        {
            if (Account is null)
                return;

            activePage--;
            if (activePage < 0)
                activePage = pages.Count - 1;

            await _persistantCache.SetAsync<int>($"{nameof(TileContentData)}.{Account.AccountType}.{Account.Id}.CurrentPage", activePage);
        }

        private void SetPage(int pageNum)
        {
            activePage = pageNum;
        }
    }
}