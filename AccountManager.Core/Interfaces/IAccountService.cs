using AccountManager.Core.Models;

namespace AccountManager.Core.Interfaces
{
    public interface IAccountService
    {
        Task<List<Account>> GetAllAccountsAsync();
        Task<List<Account>> GetAllAccountsMinAsync();
        Task LoginAsync(Account account);
        Task WriteAllAccountsAsync(List<Account> accounts);
    }
}