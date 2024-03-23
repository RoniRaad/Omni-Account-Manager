using AccountManager.Core.Models.AppSettings;
using CliWrap;
using LazyCache;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Squirrel;
using System.Diagnostics;
using System.Reflection;

namespace AccountManager.Infrastructure.Services
{
    public sealed class SquirrelAppUpdateService : IAppUpdateService
    {
		const string downgradePackageDirectoryName = "downgradePackage";
		const string firstVersionWithUpdateFix = "1.19.3";

		private readonly AboutEndpoints _endpoints;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SquirrelAppUpdateService> _logger;
        private readonly AsyncLazy<UpdateManager> _updateManager;
        public SquirrelAppUpdateService(IOptions<AboutEndpoints> endpoints, ILogger<SquirrelAppUpdateService> logger, IHttpClientFactory httpClientFactory)
        {
            _endpoints = endpoints.Value;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
			_updateManager = new AsyncLazy<UpdateManager>(async () => await UpdateManager.GitHubUpdateManager(_endpoints.Github));
		}

		public async Task<bool> CheckForUpdate()
        {
            try
            {
				var manager = await _updateManager.Value;
				if (!manager.IsInstalledApp)
				{
					_logger.LogWarning("Skipping update check. App is not installed using squirrel.");
					return false;
				}

				var currentVersion = manager.CurrentlyInstalledVersion();
                var version = manager.RootAppDirectory;

				// This is here temporarily to fix an accidental large jump in minor version number. TODO: remove this
				if (currentVersion.Version.Minor == 91)
				{
                    await ForceUpdateToVersion(new NuGet.SemanticVersion(firstVersionWithUpdateFix));
				}

				var updateInfo = await manager.CheckForUpdate();

				if (updateInfo.ReleasesToApply.Count > 0 && updateInfo.FutureReleaseEntry.Version.Version.Minor != 91)
                {
                    _logger.LogInformation("Update found, showing update message.");
                    return true;
                }

                _logger.LogInformation("No updates found.");
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
				var manager = await _updateManager.Value;

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
			var manager = await _updateManager.Value;
			var currentApplicationDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? ".";
            var rootAppDirectory = manager.RootAppDirectory;

            await ForceDownloadPackageVersion(version, rootAppDirectory);
			CleanupAfterForceUpdate(currentApplicationDirectory, rootAppDirectory);

            Environment.Exit(0);
		}

        public async Task ForceDownloadPackageVersion(NuGet.SemanticVersion version, string rootAppDirectory)
        {
			var versionString = $"{version.Version.Major}.{version.Version.Minor}.{version.Version.Build}";
			var githubReleasePackageUri = $"https://github.com/RoniRaad/Omni-Account-Manager/releases/download/v{versionString}/OmniAccountManager-{versionString}-full.nupkg";
			var downgradePackageDirectory = Path.Combine(rootAppDirectory, downgradePackageDirectoryName);
			var updateExePath = Path.Combine(rootAppDirectory, "Update.exe");

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
				Directory.Delete(Path.Combine(rootAppDirectory, "packages"), true);
			}
			catch
			{

			}

			var downloadCorrectVersionStartInfo = new ProcessStartInfo(updateExePath, exeArguments)
			{
				UseShellExecute = false,
				CreateNoWindow = true,
				WindowStyle = ProcessWindowStyle.Hidden,
				WorkingDirectory = rootAppDirectory,
			};

			var process = Process.Start(downloadCorrectVersionStartInfo);
			if (process is null)
				return;

            await process.WaitForExitAsync();
		}

        public static void CleanupAfterForceUpdate(string currentApplicationDirectory, string rootAppDirectory)
        {
			string batchScript = Path.Combine(rootAppDirectory, "deleteDir.bat");
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
				WorkingDirectory = rootAppDirectory,
			};

			Process.Start(removeCurrentInstallStartInfo);
		}

        public async Task Restart()
        {
            try
            {
                var manager = await _updateManager.Value;
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
