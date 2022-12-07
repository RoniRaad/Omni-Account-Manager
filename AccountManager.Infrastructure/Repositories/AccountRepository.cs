using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Models.AppSettings;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;

namespace AccountManager.Infrastructure.Repositories
{
    public class AccountSqliteRepository : IAccountRepository
    {
        private readonly AccountSqliteDatabaseConfig _databaseConfig;
        private readonly IAuthService _authService;
        private readonly ILogger<AccountSqliteRepository> _logger;
        private readonly IDataMigrationService _dataMigrationService;
        private IDbConnection? _connection;

        public AccountSqliteRepository(IOptions<AccountSqliteDatabaseConfig> accountSqliteDbConfig, IAuthService authService,
            ILogger<AccountSqliteRepository> logger, IDataMigrationService dataMigrationService)
        {
            _databaseConfig = accountSqliteDbConfig.Value;
            _authService = authService;
            _logger = logger;
            _dataMigrationService = dataMigrationService;
        }

        public async Task<Account?> Get(Guid id)
        {
            if (_connection is null)
            {
                await InitializeDatabaseAsync();
            }

            var sql = "SELECT * FROM Account WHERE Id=@Id;";
            try
            {
                var account = await _connection.QuerySingleAsync<Account>(sql, new { Id = id });
                return account;
            }
            catch
            {
                _logger.LogError("Unable to get account with id {Id}", id);
                return null;
            }

        }

        public async Task<List<Account>> GetAll()
        {
            if (_connection is null)
            {
                await InitializeDatabaseAsync();
            }

            var sql = "SELECT * FROM Account;";
            try
            {
                var accounts = await _connection.QueryAsync<Account>(sql);
                return accounts.ToList();
            }
            catch
            {
                _logger.LogError("Unable to get all accounts");
                throw;
            }

        }

        public async Task<Account> Create(Account account)
        {
            if (_connection is null)
            {
                await InitializeDatabaseAsync();
            }

            _logger.LogInformation("Attempting to create account with username {Username}", account.Username);

            try
            {
                var sql = "INSERT INTO Account(Id, Name, PlatformId, Username, Password, AccountType) VALUES (@Id, @Name, @PlatformId, @Username, @Password, @AccountType);";
                await _connection.ExecuteAsync(sql, account);
            }
            catch
            {
                _logger.LogError("Unable to create account with username {Username}", account.Username);
                throw;
            }

            return account;
        }

        public async Task<Account> Update(Account account)
        {
            if (_connection is null)
            {
                await InitializeDatabaseAsync();
            }

            _logger.LogInformation("Attempting to update account with username {Username}", account.Username);

            var sql = "UPDATE Account SET Name=@Name, PlatformId=@PlatformId , Username=@Username , Password=@Username, AccountType=@Username WHERE Id=@Id";

            try
            {
                await _connection.ExecuteAsync(sql, account);
            }
            catch
            {
                _logger.LogError("Unable to update account with username {Username}", account.Username);
                throw;
            }

            return account;
        }

        public async Task Delete(Guid id)
        {
            if (_connection is null)
            {
                await InitializeDatabaseAsync();
            }

            _logger.LogInformation("Attempting to delete account with id {Guid}", id);

            var sql = "DELETE Account WHERE Id=@Id";
            try
            {
                await _connection.ExecuteAsync(sql, new { Id = id });
            }
            catch
            {
                _logger.LogError("Unable to delete account with id {Id}", id);
                throw;
            }
        }

        private async Task InitializeDatabaseAsync()
        {
            if (!_authService.LoggedIn)
            {
                throw new InvalidOperationException("User not signed in! Unable to intialize Database.");
            }

            _logger.LogInformation("Attempting to initialize account database");

            var connectionString = string.Format("Data Source={0};Password={1};", _databaseConfig.FileName, _authService.PasswordHash);
            _connection = new SqliteConnection(connectionString);

            try
            {
                await _connection.QueryAsync("CREATE TABLE IF NOT EXISTS Account(Id VARCHAR(36) PRIMARY KEY, Name VARCHAR(255),PlatformId VARCHAR(255), Username VARCHAR(255), Password  VARCHAR(255), AccountType INT);");

                var count = _connection.QuerySingle<int>("SELECT COUNT(*) FROM Account");
                if (count == 0)
                {
                    try
                    {
                        var accounts = await _dataMigrationService.GetAccountsFromEncryptedJsonFile();
                        if (accounts?.Count > 0)
                        {
                            foreach(var account in accounts)
                            {
                                await Create(account);
                            }
                        }
                    }
                    catch
                    {
                        // If unable to migrate to sql continue.
                    }
                }

            }
            catch
            {
                _logger.LogError("Unable to initialize Database!");
                throw;
            }

        }
    }
}
