using AccountManager.Core.Models.AppSettings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Squirrel;

namespace AccountManager.Infrastructure.Services
{
    public class SquirrelAppUpdateService : IAppUpdateService
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
                using (var manager = await UpdateManager.GitHubUpdateManager(_endpoints.Github))
                {
                    var updateInfo = await manager.CheckForUpdate();
                    if (updateInfo.ReleasesToApply.Count > 0)
                    {
                        return true;
                    }

                    return false;
                }
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
                using (var manager = await UpdateManager.GitHubUpdateManager(_endpoints.Github))
                {
                    await manager.UpdateApp();
                    UpdateManager.RestartApp();
                }
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
