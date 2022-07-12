using Microsoft.AspNetCore.Components;
using AccountManager.Core.Models;
using Blazorise.Charts;
using AccountManager.Core.Models.Steam;
using Microsoft.Extensions.Caching.Distributed;
using AccountManager.Core.Static;

namespace AccountManager.Blazor.Components.AccountListTile.TileContent.Pages.Steam
{
    public partial class SteamFrontPage
    {
        public static int OrderNumber = 0;
        private Account _account = new();
        private bool steamInstallNotFound = false;
        [Parameter]
        public Account Account { get; set; } = new();
        List<SteamGameManifest> Games { get; set; } = new();
        public string SelectedSteamGame = "none";

        public void SetGame(string appId)
        {
            _persistantCache.SetString($"{Account.Guid}.SelectedSteamGame", appId);
        }

        public void OnRadioClicked(ChangeEventArgs args)
        {
            SetGame(args?.Value?.ToString() ?? "none");
            SelectedSteamGame = args?.Value?.ToString() ?? "none";
        }

        protected override void OnInitialized()
        {
            _account = Account;
        }

        protected override async Task OnParametersSetAsync()
        {
            if (_account != Account)
            {
                _account = Account;
            }

            await base.OnParametersSetAsync();
        }

        public async Task RefreshGame()
        {
            SelectedSteamGame = await _persistantCache.GetStringAsync($"{Account.Guid}.SelectedSteamGame") ?? "none";
            var libraryDirectories = _userSettings.Settings.SteamLibraryDirectories;
            if (!File.Exists(Path.Combine(_userSettings.Settings.SteamInstallDirectory, "steam.exe")))
            {
                steamInstallNotFound = true;
            }
            foreach (var library in libraryDirectories)
            {
                foreach (var manifestDirectory in SteamFileSystemHelper.GetInstalledGamesManifest(library))
                {
                    try
                    {
                        var deserializedManifest = await SteamFileSystemHelper.ParseGameManifest(manifestDirectory);
                        Games.Add(deserializedManifest);
                    }
                    catch
                    {
                    }

                }
            }

            Games.RemoveAll(game => game.name == "Steamworks Common Redistributables");
        }

        protected async override Task OnInitializedAsync()
        {
            await RefreshGame();
            await base.OnInitializedAsync();
        }
    }
}