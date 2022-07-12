using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using Microsoft.Extensions.Caching.Distributed;
using System.Diagnostics;

namespace AccountManager.Infrastructure.Services.Platform
{
    public class SteamPlatformService : IPlatformService
    {
        private readonly IDistributedCache _persistantCache;

        public SteamPlatformService(IDistributedCache persistantCache)
        {
            _persistantCache = persistantCache;
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
            Process.Start($"{FindSteamDrive()}\\Program Files (x86)\\Steam\\steam.exe", args);
        }
        public DriveInfo? FindSteamDrive()
        {
            DriveInfo? steamDrive = DriveInfo.GetDrives().FirstOrDefault(
                (drive) => Directory.Exists($"{drive?.RootDirectory}\\Program Files (x86)\\Steam"), null);

            return steamDrive;
        }
        public async Task LoginAsync(string userName, string password, string args)
        {
            await Task.Run(() =>
            {
                StopSteam();
                StartSteam($"{args} -login {userName} {password}");
            });

        }
        public async Task Login(Account account)
        {
            var args = "";
            var steamGameToLaunch = await _persistantCache.GetStringAsync($"{account.Guid}.SelectedSteamGame");
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
            return Task.FromResult<(bool, string)>(new (true,string.Empty));
        }

        public Task<(bool, Graphs)> TryFetchRankedGraphs(Account account)
        {
            return Task.FromResult<(bool, Graphs)>((true, new()));
        }
    }
}
