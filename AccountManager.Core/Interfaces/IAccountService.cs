using AccountManager.Core.Models;

namespace AccountManager.Core.Interfaces
{
    public interface IAccountService
    {
        Task DeleteAccountAsync(Account account);
        Task<Account?> GetAccountAsync(Guid id);
        Task<List<Account>> GetAllAccountsAsync();
        Task LoginAsync(Account account);
        Task SaveAccountAsync(Account account);
    }
}