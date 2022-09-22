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
        private Account _account = new();
        private bool steamInstallNotFound = false;
        [Parameter]
        public Account Account { get; set; } = new();
        List<SteamGameManifest> Games { get; set; } = new();

        protected override void OnInitialized()
        {
            _account = Account;
        }

        protected override async Task OnParametersSetAsync()
        {
            _account = Account;

            await base.OnParametersSetAsync();
        }

        public void SetGame(string appId)
        {
            _persistantCache.SetString($"{Account.Guid}.SelectedSteamGame", appId);
        }

        public void OnRadioClicked(ChangeEventArgs args)
        {
            SetGame(args?.Value?.ToString() ?? "none");
            selectedSteamGame = args?.Value?.ToString() ?? "none";
        }
        public async Task RefreshGamesAsync()
        {
            Games.Clear();

            selectedSteamGame = await _persistantCache.GetStringAsync($"{Account.Guid}.SelectedSteamGame") ?? "none";
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
            var cachedSelectedGame = await _persistantCache.GetStringAsync($"{Account.Guid}.SelectedSteamGame") ?? "none";
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
        }
    }
}