using Microsoft.AspNetCore.Components;
using AccountManager.Core.Models;
using Microsoft.Extensions.Caching.Distributed;
using AccountManager.Core.Attributes;
using AccountManager.Core.Models.EpicGames;

namespace AccountManager.Blazor.Components.Modals.SingleAccountModal.Pages.EpicGames
{
    [SingleAccountPage("Game Select", Core.Enums.AccountType.EpicGames, 0)]
    public partial class EpicGamesGameSelectPage
    {
        [Parameter, EditorRequired]
        public Account? Account { get; set; }

        [Parameter, EditorRequired]
        public Action? IncrementPage { get; set; }

        [Parameter, EditorRequired]
        public Action? DecrementPage { get; set; }

        private List<EpicGamesInstalledGame> games = new();
        private bool epicInstallNotFound = false;
        private string selectedEpicGame = "none";

        protected override async Task OnInitializedAsync()
        {
            if (Account is null)
                return;

            await RefreshGame();
        }

        public void SetGame(string appId)
        {
            _persistantCache.SetString($"{Account?.Id}.SelectedEpicGame", appId);
        }

        public void OnRadioClicked(ChangeEventArgs args)
        {
            SetGame(args?.Value?.ToString() ?? "none");
            selectedEpicGame = args?.Value?.ToString() ?? "none";
        }
        public async Task RefreshGame()
        {
            games.Clear();

            selectedEpicGame = await _persistantCache.GetStringAsync($"{Account?.Id}.SelectedEpicGame") ?? "none";

            if (!_epicLibraryService.TryGetInstalledGames(out var gameManifests))
                return;

            games.AddRange(gameManifests);
        }
    }
}