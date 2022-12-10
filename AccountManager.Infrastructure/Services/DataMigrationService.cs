using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;

namespace AccountManager.Infrastructure.Services
{
    public class DataMigrationService : IDataMigrationService
    {
        private readonly IGeneralFileSystemService _fileSystemService;
        private readonly ILogger<DataMigrationService> _logger;
        public DataMigrationService(IGeneralFileSystemService fileSystemService,
            ILogger<DataMigrationService> logger)
        {
            _fileSystemService = fileSystemService;
            _logger = logger;
        }

        public async Task<List<Account>?> GetAccountsFromEncryptedJsonFile(string password)
        {
            _logger.LogInformation("Attempting to get accounts from encrypted json file...");

            try
            {
                var accounts = await _fileSystemService.ReadDataAsync<List<Account>>(password);
                return accounts;
            }
            catch
            {
                _logger.LogError("Unable to get accounts from encrypted json file...");

                return null;
            }
        }

        public bool TryDecryptJsonFile(string password)
        {
            _logger.LogInformation("Attempting to decrypt json file...");

            try
            {
                _fileSystemService.ReadData<List<Account>>(password);
                return true;
            }
            catch
            {
                _logger.LogError("Unable to decrypt json file...");

                return false;
            }
        }
    }
}
