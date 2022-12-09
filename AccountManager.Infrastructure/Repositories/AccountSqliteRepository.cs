using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Models.AppSettings;
using AccountManager.Core.Static;
using AccountManager.Infrastructure.Services.FileSystem;
using Dapper;
using KeyedSemaphores;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;

namespace AccountManager.Infrastructure.Repositories
{
    public class AccountSqliteRepository : IAccountEncryptedRepository
    {
        private readonly AccountSqliteDatabaseConfig _databaseConfig;
        private readonly ILogger<AccountSqliteRepository> _logger;
        private readonly IDataMigrationService _dataMigrationService;
        private readonly string DatabasePath;
        private bool Initialized = false;


        public AccountSqliteRepository(IOptions<AccountSqliteDatabaseConfig> accountSqliteDbConfig,
            ILogger<AccountSqliteRepository> logger, IDataMigrationService dataMigrationService)
        {
            _databaseConfig = accountSqliteDbConfig.Value;
            _logger = logger;
            _dataMigrationService = dataMigrationService;
            DatabasePath = Path.Combine(GeneralFileSystemService.DataPath, _databaseConfig.FileName);
        }

        public async Task<Account?> Get(Guid id, string password)
        {
            if (!Initialized)
            {
                await InitializeDatabaseAsync(password);
            }

            var connectionString = string.Format("Data Source={0};Password={1};", DatabasePath, password);
            using var connection = new SqliteConnection(connectionString);

            var sql = "SELECT * FROM Account WHERE Id=@Id;";
            try
            {
                var account = await connection.QuerySingleOrDefaultAsync<Account>(sql, new { Id = id });
                return account;
            }
            catch
            {
                _logger.LogError("Unable to get account with id {Id}", id);
                return null;
            }

        }

        public async Task<List<Account>> GetAll(string password)
        {
            if (!Initialized)
            {
                await InitializeDatabaseAsync(password);
            }

            var connectionString = string.Format("Data Source={0};Password={1};", DatabasePath, password);
            using var connection = new SqliteConnection(connectionString);

            var sql = "SELECT * FROM Account;";
            try
            {
                var accounts = await connection.QueryAsync<Account>(sql);
                return accounts.ToList();
            }
            catch
            {
                _logger.LogError("Unable to get all accounts");
                throw;
            }

        }

        public async Task<Account> Create(Account account, string password)
        {
            if (!Initialized)
            {
                await InitializeDatabaseAsync(password);
            }

            var connectionString = string.Format("Data Source={0};Password={1};", DatabasePath, password);
            using var connection = new SqliteConnection(connectionString);

            _logger.LogInformation("Attempting to create account with username {Username}", account.Username);

            try
            {
                var sql = "INSERT INTO Account(Id, Name, PlatformId, Username, Password, AccountType) VALUES (@Id, @Name, @PlatformId, @Username, @Password, @AccountType);";
                await connection.ExecuteAsync(sql, account);
            }
            catch
            {
                _logger.LogError("Unable to create account with username {Username}", account.Username);
                throw;
            }

            return account;
        }

        public async Task<Account> Update(Account account, string password)
        {
            if (!Initialized)
            {
                await InitializeDatabaseAsync(password);
            }

            var connectionString = string.Format("Data Source={0};Password={1};", DatabasePath, password);
            using var connection = new SqliteConnection(connectionString);

            _logger.LogInformation("Attempting to update account with username {Username}", account.Username);

            var sql = "UPDATE Account SET Name=@Name, PlatformId=@PlatformId , Username=@Username , Password=@Password, AccountType=@AccountType WHERE Id=@Id";

            try
            {
                await connection.ExecuteAsync(sql, account);
            }
            catch
            {
                _logger.LogError("Unable to update account with username {Username}", account.Username);
                throw;
            }

            return account;
        }

        public async Task Delete(Guid id, string password)
        {
            if (!Initialized)
            {
                await InitializeDatabaseAsync(password);
            }

            var connectionString = string.Format("Data Source={0};Password={1};", DatabasePath, password);
            using var connection = new SqliteConnection(connectionString);

            _logger.LogInformation("Attempting to delete account with id {Guid}", id);

            var sql = "DELETE FROM Account WHERE Id=@Id";
            try
            {
                var rows = await connection.ExecuteAsync(sql, new { Id = id });
                _ = "";
            }
            catch
            {
                _logger.LogError("Unable to delete account with id {Id}", id);
                throw;
            }
        }

        public bool TryDecrypt(string password)
        {
            _logger.LogInformation("Attempting to decrypt account database");

            if (File.Exists(DatabasePath))
            {
                var connectionString = string.Format("Data Source={0};Password={1};", DatabasePath, password);
                using var connection = new SqliteConnection(connectionString);

                try
                {
                    connection.Open();
                    connection.Close();
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            return _dataMigrationService.TryDecryptJsonFile(password);
        }

        public bool TryChangePassword(string oldPassword, string newPassword)
        {
            _logger.LogInformation("Attempting to decrypt account database");

            var connectionString = string.Format("Data Source={0};Password={1};", DatabasePath, oldPassword);
            using var connection = new SqliteConnection(connectionString);

            try
            {
                connection.Execute($"PRAGMA rekey='{newPassword}';", new {NewPassword = newPassword});
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task InitializeDatabaseAsync(string password)
        {
            using (await KeyedSemaphore.LockAsync("SqliteDbInit"))
            {

                _logger.LogInformation("Attempting to initialize account database");

                var connectionString = string.Format("Data Source={0};Password={1};", DatabasePath, password);
                using var connection = new SqliteConnection(connectionString);

                try
                {
                    await connection.QueryAsync("CREATE TABLE IF NOT EXISTS Account(Id VARCHAR(36) PRIMARY KEY, Name VARCHAR(255),PlatformId VARCHAR(255), Username VARCHAR(255), Password  VARCHAR(255), AccountType INT);");

                    var count = connection.QuerySingle<int>("SELECT COUNT(*) FROM Account");
                    if (count == 0)
                    {
                        try
                        {
                            Initialized = true;
                            var accounts = await _dataMigrationService.GetAccountsFromEncryptedJsonFile(password);
                            if (accounts?.Count > 0)
                            {
                                foreach (var account in accounts)
                                {
                                    await Create(account, password);
                                }
                            }
                        }
                        catch
                        {
                            // If unable to migrate to sql continue.
                        }
                    }

                    Initialized = true;
                }
                catch
                {
                    _logger.LogError("Unable to initialize Database!");
                    throw;
                }
            }
        }
    }
}
