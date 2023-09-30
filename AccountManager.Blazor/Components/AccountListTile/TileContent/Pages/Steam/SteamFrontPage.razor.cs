using Microsoft.AspNetCore.Components;
using AccountManager.Core.Models;
using Blazorise.Charts;
using AccountManager.Core.Models.Steam;
using Microsoft.Extensions.Caching.Distributed;
using AccountManager.Core.Static;
using AccountManager.Core.Attributes;

namespace AccountManager.Blazor.Components.AccountListTile.TileContent.Pages.Steam
{
    [AccountTilePage(Core.Enums.AccountType.Steam, 0)]
    public partial class SteamFrontPage
    {
        private string selectedSteamGame = "none";
        private Account? _account = null;
        private bool steamInstallNotFound = false;
        [CascadingParameter]
        public Account? Account { get; set; }
        List<SteamGameManifest> Games { get; set; } = new();
        [CascadingParameter(Name = "RegisterTileDataRefresh")]
        Action<Action> RegisterTileDataRefresh { get; set; } = delegate { };
        protected override async Task OnParametersSetAsync()
        {
            if (Account is null || Account != _account)
                return;

            _account = Account;

            await base.OnParametersSetAsync();
        }

        public void SetGame(string appId)
        {
            if (Account is null)
                return;

            _persistantCache.SetString($"{Account.Id}.SelectedSteamGame", appId);
        }

        public void OnRadioClicked(ChangeEventArgs args)
        {
            SetGame(args?.Value?.ToString() ?? "none");
            selectedSteamGame = args?.Value?.ToString() ?? "none";
        }
        public async Task RefreshGamesAsync()
        {
            if (Account is null)
                return;

            Games.Clear();

            selectedSteamGame = await _persistantCache.GetStringAsync($"{Account.Id}.SelectedSteamGame") ?? "none";
            if (!File.Exists(Path.Combine(_generalSettings.Settings.SteamInstallDirectory, "steam.exe")))
            {
                steamInstallNotFound = true;
            }

            if (_steamLibraryService.TryGetGameManifests(out var gameManifests))
            {
                Games.AddRange(gameManifests);
            }

            Games.RemoveAll(game => game.Name == "Steamworks Common Redistributables" || (game.LastOwner != Account.PlatformId && _steamSettings.Settings.OnlyShowOwnedSteamGames));
        }

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (Account is null)
                return;

            var cachedSelectedGame = await _persistantCache.GetStringAsync($"{Account.Id}.SelectedSteamGame") ?? "none";
            if (cachedSelectedGame != selectedSteamGame)
            {
                selectedSteamGame = cachedSelectedGame;
                StateHasChanged();
            }
            await base.OnAfterRenderAsync(firstRender);
        }

        protected async override Task OnInitializedAsync()
        {
            await RefreshGamesAsync();
            await base.OnInitializedAsync();
            RegisterTileDataRefresh(() => Task.Run(RefreshGamesAsync));
        }
    }
}