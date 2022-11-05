namespace AccountManager.Core.Interfaces
{
    public interface IAccountExportService
    {
        Task ExportAccountsAsync(List<string> accountIds, string password, string filePath);
        Task ImportAccountsAsync(string filePath, string password);
    }
}