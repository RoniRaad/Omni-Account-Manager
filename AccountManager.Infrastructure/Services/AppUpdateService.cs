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
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SquirrelAppUpdateService> _logger;
        public SquirrelAppUpdateService(IOptions<AboutEndpoints> endpoints, ILogger<SquirrelAppUpdateService> logger, IHttpClientFactory httpClientFactory)
        {
            _endpoints = endpoints.Value;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
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
                    await ForceUpdateToVersion(new NuGet.SemanticVersion("1.19.2"));
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

        public async Task ForceUpdateToVersion(NuGet.SemanticVersion version)
        {
            const string downgradePackageDirectoryName = "downgradePackage";

            var currentApplicationDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? ".";
            string updateExePath = Path.Combine(currentApplicationDirectory, "..", "Update.exe");
            string updateExeDirectory = Path.GetDirectoryName(updateExePath) ?? "..";
            var downgradePackageDirectory = Path.Combine(updateExeDirectory, downgradePackageDirectoryName);
            var versionString = $"{version.Version.Major}.{version.Version.Minor}.{version.Version.Build}";
            var githubReleasePackageUri = $"https://github.com/RoniRaad/Omni-Account-Manager/releases/download/v{versionString}/OmniAccountManager-{versionString}-full.nupkg";
            
            var httpClient = _httpClientFactory.CreateClient();
            var getNuGetPackage = await httpClient.GetAsync(githubReleasePackageUri);

            if (!Directory.Exists(downgradePackageDirectory))
            {
                Directory.CreateDirectory(downgradePackageDirectory);
            }

            using (FileStream nugetFile = File.OpenWrite($"{downgradePackageDirectory}/OmniAccountManager-{versionString}-full.nupkg"))
            {
                using var contentStream = getNuGetPackage.Content.ReadAsStream();
                await contentStream.CopyToAsync(nugetFile);
            }


            string exeArguments = $"--update={downgradePackageDirectoryName}";

            try
            {
                Directory.Delete(Path.Combine(updateExeDirectory, "packages"), true);
            }
            catch
            {

            }

            var downloadCorrectVersionStartInfo = new ProcessStartInfo(updateExePath, exeArguments)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = updateExeDirectory,
            };

            var process = Process.Start(downloadCorrectVersionStartInfo);
            if (process is null)
                return;

            string batchScript = Path.Combine(currentApplicationDirectory, "deleteDir.bat");
            using (StreamWriter sw = new StreamWriter(batchScript))
            {
                sw.WriteLine("@echo off");
                sw.WriteLine($"powershell.exe -Command \"Start-Sleep -Seconds 5; Remove-Item -Recurse -Force '{currentApplicationDirectory}'\""); // Wait for 5 seconds to ensure the app has closed
                sw.WriteLine("start OmniAccountManager.exe"); // start the app
                sw.WriteLine($"powershell.exe -Command \"Remove-Item -Recurse -Force '{downgradePackageDirectoryName}'\""); // remove downgrade folder
                sw.WriteLine("del \"%~f0\""); // Self-delete the batch script
            }

            // Configure and start the batch script process
            ProcessStartInfo removeCurrentInstallStartInfo = new ProcessStartInfo(batchScript)
            {
                UseShellExecute = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = updateExeDirectory,
            };

            Process.Start(removeCurrentInstallStartInfo);
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
