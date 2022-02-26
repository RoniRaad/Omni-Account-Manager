using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace AccountManager.Infrastructure.Services
{
    public class SteamLoginService : ILoginService
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
        public DriveInfo FindSteamDrive()
        {
            DriveInfo steamDrive = null;
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (Directory.Exists($"{drive.RootDirectory}\\Program Files (x86)\\Steam"))
                {
                    steamDrive = drive;
                }
            }
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
    }
}
