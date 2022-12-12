using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Models.UserSettings;
using AccountManager.Core.Static;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;

namespace AccountManager.Infrastructure.Services.Platform
{
    public sealed class EpicGamesPlatformService : IPlatformService
    {
        private static readonly string EncryptionKey = "A09C853C9E95409BB94D707EADEFA52E";
        public readonly static string WebIconFilePath = Path.Combine("logos", "epic-games-logo.png");
        public readonly static string IcoFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location)
            ?? ".", "ShortcutIcons", "epic-logo.ico");
        private readonly IAlertService _alertService;
        private readonly ILogger<EpicGamesPlatformService> _logger;
        private readonly IDistributedCache _persistantCache;
        private readonly IEpicGamesLibraryService _epicGamesLibraryService;
        private readonly IUserSettingsService<GeneralSettings> _settingsService;
        private readonly IEpicGamesExternalAuthService _epicGamesExternalAuthService;
        private readonly IAccountService _accountService;
        private readonly IAppState _appState;

        public EpicGamesPlatformService(IAlertService alertService,
            IUserSettingsService<GeneralSettings> settingsService, ILogger<EpicGamesPlatformService> logger,
            IEpicGamesExternalAuthService epicGamesExternalAuthService, IDistributedCache persistantCache, 
            IEpicGamesLibraryService epicGamesLibraryService, IAppState appState, IAccountService accountService)
        {
            _alertService = alertService;
            _settingsService = settingsService;
            _logger = logger;
            _epicGamesExternalAuthService = epicGamesExternalAuthService;
            _persistantCache = persistantCache;
            _epicGamesLibraryService = epicGamesLibraryService;
            _appState = appState;
            _accountService = accountService;
        }

        public async Task Login(Account account)
        {
            var tokens = await _epicGamesExternalAuthService.TryGetEpicGamesAccessTokens(account.Username, account.Password);
            if (tokens?.RefreshToken is null)
            {
                _alertService.AddErrorAlert("There was an error attempting to sign in.");
                _logger.LogError("Epic games login failed! No tokens were returned!");
                return;
            }

            if (string.IsNullOrEmpty(account.PlatformId))
            {
                account.PlatformId = tokens.Id;
                await _accountService.SaveAccountAsync(account);
            }

            CloseEpicGamesClient();
            await SetEpicGamesTokenFile(tokens.Username ?? "", tokens.Name ?? "", tokens.LastName ?? "", tokens.DisplayName ?? "", tokens.RefreshToken);
            await Task.Delay(2000);

            var gameId = await _persistantCache.GetStringAsync($"{account.Id}.SelectedEpicGame");
            if (!string.IsNullOrEmpty(gameId) && gameId != "none")
            {
                if (!TryLaunchEpicGamesGame(gameId))
                {
                    _alertService.AddErrorAlert("There was an error attempting to start the selected game.");
                    _logger.LogError("There was an error attempting to start the selected game!");
                }
                else
                    return;
            }

            if (!TryLaunchEpicGamesClient())
            {
                _alertService.AddErrorAlert("Epic games client was not found. Aborting.");
                _logger.LogError("Epic games client was not found. Aborting.");
            }
        }

        public async Task<(bool, Rank)> TryFetchRank(Account account)
        {
            var rankCacheString = $"{account.Username}.epicgames.rank";
            var rank = await _persistantCache.GetAsync<Rank>(rankCacheString);
            if ( rank is not null )
                return (true, rank);

            rank = new Rank();
            try
            {
                if (string.IsNullOrEmpty(account.PlatformId))
                    return (false, rank);

                if (!string.IsNullOrEmpty(rank?.Tier))
                    await _persistantCache.SetAsync(rankCacheString, rank, TimeSpan.FromHours(1));

                if (rank is null)
                    return (false, new Rank());

                return (true, rank);
            }
            catch
            {
                return (false, new Rank());
            }
        }

        public Task<(bool, string)> TryFetchId(Account account)
        {
            try
            {
                if (!string.IsNullOrEmpty(account.PlatformId))
                {
                    return Task.FromResult((true, account.PlatformId));
                }

                return Task.FromResult((false, string.Empty));
            }
            catch
            {
                return Task.FromResult((false, string.Empty));
            }
        }

        private async Task SetEpicGamesTokenFile(string email, string fName, string lName, string dName, string token)
        {
            var launcherTokenString = StringEncryption.EncryptEpicGamesData(GenerateLoginJson(email, fName, lName, dName, token), EncryptionKey);
            var localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var path = Path.Combine(localAppDataPath, "EpicGamesLauncher", "Saved", "Config", "Windows", "GameUserSettings.ini");
            if (File.Exists(path))
            {
                var lines = File.ReadAllLines(path);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i] == "[RememberMe]")
                    {
                        lines[i + 1] = $"Data={launcherTokenString}";
                        lines[i + 2] = "Enable=True";
                    }

                }

                await File.WriteAllLinesAsync(path, lines);
            }
        }

        private static void CloseEpicGamesClient()
        {
            foreach (var process in Process.GetProcesses()
                .Where((process) => process.ProcessName.ToLower().Contains("epicgames")))
            {
                process.Kill();
            }
        }

        private bool TryLaunchEpicGamesClient()
        {
            var epicInstallPath = _settingsService.Settings.EpicGamesInstallDirectory;
            string defaultEpicPath = Path.Combine(epicInstallPath, "Launcher", "Portal", "Binaries", "Win32", "EpicGamesLauncher.exe");
            if (!File.Exists(defaultEpicPath))
                return false;

            Process.Start(defaultEpicPath);
            return true;
        }

        private bool TryLaunchEpicGamesGame(string appId)
        {
            if (!_epicGamesLibraryService.TryGetInstalledGames(out var installedGames))
                return false;

            var game = installedGames.FirstOrDefault((game) => game.AppId == appId);
            if (game?.InstallLocation is not null && game?.LaunchExecutable is not null)
            {
                var processStart = new ProcessStartInfo();
                processStart.WorkingDirectory = game.InstallLocation;
                processStart.FileName = Path.Combine(game.InstallLocation, game.LaunchExecutable);
                Process.Start(processStart);
            }
            else
                return false;

            return true;
        }

        private string GenerateLoginJson(string email, string fName, string lName, string dName, string token)
        {
            return $"[{{\"Region\":\"Prod\",\"Email\":\"{email}\",\"Name\":\"{fName}\",\"LastName\":\"{lName}\",\"DisplayName\":\"{dName}\",\"Token\":\"{token}\",\"bHasPasswordAuth\":true}}]\0";
        }
    }
}
