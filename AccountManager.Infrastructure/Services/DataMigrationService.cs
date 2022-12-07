using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;

namespace AccountManager.Infrastructure.Services
{
    public class DataMigrationService : IDataMigrationService
    {
        private readonly IAuthService _authService;
        private readonly IGeneralFileSystemService _fileSystemService;
        private readonly ILogger<DataMigrationService> _logger;
        public DataMigrationService(IAuthService authService, IGeneralFileSystemService fileSystemService,
            ILogger<DataMigrationService> logger)
        {
            _authService = authService;
            _fileSystemService = fileSystemService;
            _logger = logger;
        }

        public async Task<List<Account>?> GetAccountsFromEncryptedJsonFile()
        {
            if (!_authService.LoggedIn)
            {
                return null;
            }

            _logger.LogInformation("Attempting to migrate accounts to sql...");

            try
            {
                var accounts = await _fileSystemService.ReadDataAsync<List<Account>>(_authService.PasswordHash);
                return accounts;
            }
            catch
            {
                return null;
            }
        }
    }
}
