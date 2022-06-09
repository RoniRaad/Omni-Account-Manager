using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using System.Diagnostics;

namespace AccountManager.Infrastructure.Services.Platform
{
    public class SteamPlatformService : IPlatformService
    {
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
            await LoginAsync(account.Username, account.Password, "");
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

        public async Task<(bool, Graphs)> TryFetchRankedGraphs(Account account)
        {
            return (true, new());
        }
    }
}
