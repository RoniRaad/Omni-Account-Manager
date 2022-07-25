using Microsoft.AspNetCore.Components;
using AccountManager.Core.Models;
using AccountManager.Core.Models.RiotGames.Valorant.Responses;
using AccountManager.Core.Models.Steam;
using Microsoft.Extensions.Caching.Distributed;

namespace AccountManager.Blazor.Components.Modals.SingleAccountModal.Pages.Steam
{
    public partial class SteamGameSelectPage
    {
        [Parameter, EditorRequired]
        public Account? Account { get; set; }

        [Parameter, EditorRequired]
        public Action? IncrementPage { get; set; }

        [Parameter, EditorRequired]
        public Action? DecrementPage { get; set; }

        public static string Title = "Game Select";
        List<SteamGameManifest> games = new();
        bool steamInstallNotFound = false;
        public string SelectedSteamGame = "none";

        protected override async Task OnInitializedAsync()
        {
            if (Account is null)
                return;

            await RefreshGame();
        }

        public void SetGame(string appId)
        {
            _persistantCache.SetString($"{Account?.Guid}.SelectedSteamGame", appId);
        }

        public void OnRadioClicked(ChangeEventArgs args)
        {
            SetGame(args?.Value?.ToString() ?? "none");
            SelectedSteamGame = args?.Value?.ToString() ?? "none";
        }
        public async Task RefreshGame()
        {
            games.Clear();
            SelectedSteamGame = await _persistantCache.GetStringAsync($"{Account?.Guid}.SelectedSteamGame") ?? "none";
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