using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Models.UserSettings;
using Microsoft.Extensions.Caching.Distributed;
using System.Diagnostics;
using System.Reflection;

namespace AccountManager.Infrastructure.Services.Platform
{
    public sealed class SteamPlatformService : IPlatformService
    {
        private readonly IDistributedCache _persistantCache;
        private readonly IUserSettingsService<GeneralSettings> _userSettings;
        private readonly ISteamLibraryService _steamLibraryService;
        public static readonly string WebIconFilePath = Path.Combine("logos", "steam-logo.svg");
        public static readonly string IcoFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location)
            ?? ".", "ShortcutIcons", "steam-logo.ico");
        public SteamPlatformService(IDistributedCache persistantCache, IUserSettingsService<GeneralSettings> userSettings, ISteamLibraryService steamLibraryService)
        {
            _persistantCache = persistantCache;
            _userSettings = userSettings;
            _steamLibraryService = steamLibraryService;
        }

        public void StopSteam()
        {
            foreach (Process steamProcess in Process.GetProcessesByName("Steam"))
            {
                steamProcess.Kill();
            }
        }

        public void StartSteam(string args)
        {
            StopSteam();
            Process.Start(Path.Combine(_userSettings.Settings.SteamInstallDirectory, "steam.exe"), args);
        }

        public async Task LoginAsync(string userName, string password, string args)
        {
            await Task.Run(() =>
            {
                StopSteam();
                StartSteam($"{args} -noreactlogin -login {userName} {password}");
            });

        }

        public async Task Login(Account account)
        {
            var args = "";
            var steamGameToLaunch = await _persistantCache.GetStringAsync($"{account.Id}.SelectedSteamGame");
            if (steamGameToLaunch is not null && steamGameToLaunch != "none")
                args = $"-applaunch {steamGameToLaunch}";

            await LoginAsync(account.Username, account.Password, args);
        }

        public string GetCommandLineValue(string commandline , string key)
        {
            key += "=";
            var valueStart = commandline.IndexOf(key) + key.Length;
            var valueEnd = commandline.IndexOf(" ", valueStart);
            return commandline.Substring(valueStart, valueEnd - valueStart).Replace(@"\", "").Replace("\"", "");
        }

        public Task<(bool, Rank)> TryFetchRank(Account account)
        {
            return Task.FromResult<(bool, Rank)>(new (true, new Rank()));
        }

        public Task<(bool, string)> TryFetchId(Account account)
        {
            var getSuccessful = _steamLibraryService.TryGetUserId(account, out string userId);
            return Task.FromResult<(bool, string)>((getSuccessful, userId));
        }
    }
}
