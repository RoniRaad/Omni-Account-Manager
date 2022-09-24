using AccountManager.Core.Models.AppSettings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Squirrel;

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
                    return false;
                #endif
                using var manager = await UpdateManager.GitHubUpdateManager(_endpoints.Github);
                var updateInfo = await manager.CheckForUpdate();
                if (updateInfo.ReleasesToApply.Count > 0)
                {
                    return true;
                }

                return false;
            }
            catch
            {
                _logger.LogError("Unable to check for updates for Omni Account Manager.");
                return false;
            }
        }

        public async Task Update()
        {
            try
            {
                using var manager = await UpdateManager.GitHubUpdateManager(_endpoints.Github);
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

        public void Restart()
        {
            try
            {
                UpdateManager.RestartApp();
            }
            catch
            {
                _logger.LogError("Unable to restart Omni Account Manager.");
            }
        }
    }
}
