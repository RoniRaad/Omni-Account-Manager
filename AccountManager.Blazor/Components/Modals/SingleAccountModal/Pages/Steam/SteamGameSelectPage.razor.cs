using Microsoft.AspNetCore.Components;
using AccountManager.Core.Models;
using AccountManager.Core.Models.Steam;
using Microsoft.Extensions.Caching.Distributed;
using AccountManager.Core.Attributes;

namespace AccountManager.Blazor.Components.Modals.SingleAccountModal.Pages.Steam
{
    [SingleAccountPage("Game Select", Core.Enums.AccountType.Steam, 0)]
    public partial class SteamGameSelectPage
    {
        [CascadingParameter, EditorRequired]
        public Account? Account { get; set; }

        [Parameter, EditorRequired]
        public Action? IncrementPage { get; set; }

        [Parameter, EditorRequired]
        public Action? DecrementPage { get; set; }

        List<SteamGameManifest> games = new();
        bool steamInstallNotFound = false;
        public string SelectedSteamGame { get; set; } = "none";

        protected override async Task OnInitializedAsync()
        {
            if (Account is null)
                return;

            await RefreshGame();
        }

        public void SetGame(string appId)
        {
            _persistantCache.SetString($"{Account?.Id}.SelectedSteamGame", appId);
        }

        public void OnRadioClicked(ChangeEventArgs args)
        {
            SetGame(args?.Value?.ToString() ?? "none");
            SelectedSteamGame = args?.Value?.ToString() ?? "none";
        }
        public async Task RefreshGame()
        {
            games.Clear();
            SelectedSteamGame = await _persistantCache.GetStringAsync($"{Account?.Id}.SelectedSteamGame") ?? "none";
            if (!File.Exists(Path.Combine(_generalSettings.Settings.SteamInstallDirectory, "steam.exe")))
            {
                steamInstallNotFound = true;
            }

            if (_steamLibraryService.TryGetGameManifests(out var gameManifests))
                games.AddRange(gameManifests);

            games.RemoveAll(game => game.Name == "Steamworks Common Redistributables" || (game.LastOwner != Account?.PlatformId && _steamSettings.Settings.OnlyShowOwnedSteamGames));
        }
    }
}