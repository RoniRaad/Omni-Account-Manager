using AccountManager.Core.Models.AppSettings;
using CliWrap;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Squirrel;
using System.Diagnostics;
using System.Reflection;

namespace AccountManager.Infrastructure.Services
{
    public sealed class SquirrelAppUpdateService : IAppUpdateService
    {
        private readonly AboutEndpoints _endpoints;
        private readonly ILogger<SquirrelAppUpdateService> _logger;
        public SquirrelAppUpdateService(IOptions<AboutEndpoints> endpoints, ILogger<SquirrelAppUpdateService> logger)
        {
            _endpoints = endpoints.Value;
            _logger = logger;
        }

        public async Task<bool> CheckForUpdate()
        {
            try
            {
                #if DEBUG
                    _logger.LogError("Skipping update check due to the app running in debug mode.");
				    using var manager2 = await UpdateManager.GitHubUpdateManager(_endpoints.Github);
				    var currentVersion = manager2.CurrentlyInstalledVersion();
                 
				    // This is here temporarily to fix an accidental large jump in minor version number. TODO: remove this
				if (currentVersion.Version.Minor == 91)
				{
                    var httpClient = new HttpClient();
                    var getNugetPackage = await httpClient.GetAsync("https://github.com/RoniRaad/Omni-Account-Manager/releases/download/v1.19.2/OmniAccountManager-1.19.2-full.nupkg");
                    if (!Directory.Exists("../tempDowngrade"))
                    {
                        Directory.CreateDirectory("../tempDowngrade");
                    }
                    using (FileStream nugetFile = File.OpenWrite("../tempDowngrade/OmniAccountManager-1.19.2-full.nupkg"))
                    {
                        using var contentStream = getNugetPackage.Content.ReadAsStream();
                        await contentStream.CopyToAsync(nugetFile);
                    }


                    string exePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? ".", "..", "Update.exe");
					string exeArguments = "--update=tempDowngrade";
					string exeDirectory = Path.GetDirectoryName(exePath);

                    try
                    {
						Directory.Delete(Path.Combine(exeDirectory, "packages"), true);
					}
                    catch
                    {

                    }

					var downloadCorrectVersionStartInfo = new ProcessStartInfo(exePath, exeArguments)
					{
						UseShellExecute = false,
						CreateNoWindow = true,
						WindowStyle = ProcessWindowStyle.Hidden,
                        WorkingDirectory = exeDirectory,
					};

					var process = Process.Start(downloadCorrectVersionStartInfo);
                    await process.WaitForExitAsync();

					string batchScript = Path.Combine(exeDirectory, "deleteDir.bat");
					using (StreamWriter sw = new StreamWriter(batchScript))
					{
						sw.WriteLine("@echo off");
						sw.WriteLine($"powershell.exe -Command \"Start-Sleep -Seconds 5; Remove-Item -Recurse -Force '{Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location)}'\""); // Wait for 5 seconds to ensure the app has closed
						sw.WriteLine("start OmniAccountManager.exe"); // start the app
						sw.WriteLine("powershell.exe -Command \"Remove-Item -Recurse -Force 'tempDowngrade'\""); // remove downgrade folder
						sw.WriteLine("del \"%~f0\""); // Self-delete the batch script
					}

					// Configure and start the batch script process
					ProcessStartInfo removeCurrentInstallStartInfo = new ProcessStartInfo(batchScript)
					{
						UseShellExecute = true,
						CreateNoWindow = true,
						WindowStyle = ProcessWindowStyle.Hidden,
						WorkingDirectory = exeDirectory,
					};

					Process.Start(removeCurrentInstallStartInfo);
                    manager2.KillAllExecutablesBelongingToPackage();
				}

				    return false;
                #endif
                using var manager = await UpdateManager.GitHubUpdateManager(_endpoints.Github);
                var updateInfo = await manager.CheckForUpdate();
                if (updateInfo.ReleasesToApply.Count > 0)
                {
                    _logger.LogError("Update found, showing update message.");
                    return true;
                }

                _logger.LogError("No updates found.");
                return false;
            }
            catch
            {
                _logger.LogError("Unable to check for updates for Omni Account Manager.");
                return false;
            }
        }

        public async Task UpdateAndRestart()
        {
            try
            {

				using var manager = await UpdateManager.GitHubUpdateManager(_endpoints.Github);
				var currentVersion = manager.CurrentlyInstalledVersion();

				// This is here temporarily to fix an accidental large jump in minor version number. TODO: remove this
				if (currentVersion.Version.Minor == 91)
				{
					List<ReleaseEntry> releases = new List<ReleaseEntry>();
					var httpClient = new HttpClient();
					releases.AddRange(ReleaseEntry.ParseReleaseFile(await httpClient.GetStringAsync("https://github.com/RoniRaad/Omni-Account-Manager/releases/download/v1.91.2/OmniAccountManager-1.91.2-full.nupkg")));
					await manager.DownloadReleases(releases);
				}

				var releaseEntry = await manager.UpdateApp();
                var version = releaseEntry.Version;
                var latestExePath = Path.Combine(manager.RootAppDirectory, string.Concat("app-", version.Version.Major, ".", version.Version.Minor, ".", version.Version.Build), "OmniAccountManager.exe");

                UpdateManager.RestartApp(latestExePath);
            }
            catch
            {
                _logger.LogError("Unable to update Omni Account Manager.");
            }
        }

        public async Task Restart()
        {
            try
            {
                using var manager = await UpdateManager.GitHubUpdateManager(_endpoints.Github);
                var version = manager.CurrentlyInstalledVersion();
                var latestExePath = Path.Combine(manager.RootAppDirectory, string.Concat("app-", version.Version.Major, ".", version.Version.Minor, ".", version.Version.Build), "OmniAccountManager.exe");
                _logger.LogInformation("Attempting to restart app using path {path}", latestExePath);
                UpdateManager.RestartApp(latestExePath);
            }
            catch
            {
                _logger.LogError("Unable to restart Omni Account Manager.");
            }
        }
    }
}
