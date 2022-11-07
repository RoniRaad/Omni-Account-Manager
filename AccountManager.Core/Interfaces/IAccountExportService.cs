using AccountManager.Core.Models;

namespace AccountManager.Core.Interfaces
{
    public interface IAccountExportService
    {
        Task ExportAccountsAsync(List<Account> accounts, string password, string filePath);
        Task ImportAccountsAsync(string filePath, string password);
    }
}