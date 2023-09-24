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
                activePage = await _persistantCache.GetOrCreateAsync($"{nameof(TileContentData)}.{Account.AccountType}.{Account.Id}.CurrentPage", async () =>
                {
                    return 0;
                });
        }


        private string GetPageButtonClass(int pageNum)
        {
            if (activePage == pageNum)
                return "active";
            return "";
        }

        private async Task IncrementPage()
        {
            activePage++;
            if (activePage >= pages?.Count)
                activePage = 0;

            await _persistantCache.SetAsync<int>($"{nameof(TileContentData)}.{Account.AccountType}.{Account.Id}.CurrentPage", activePage);
        }

        private async Task DecrementPage()
        {
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