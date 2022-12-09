using AccountManager.Core.Models;

namespace AccountManager.Core.Interfaces
{
    public interface IDataMigrationService
    {
        Task<List<Account>?> GetAccountsFromEncryptedJsonFile(string password);
    }
}