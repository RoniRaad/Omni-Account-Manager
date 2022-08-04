using AccountManager.Core.Models;

namespace AccountManager.Core.Interfaces
{
    public interface IAccountService
    {
        Task<List<Account>> GetAllAccounts();
        List<Account> GetAllAccountsMin();
        Task Login(Account account);
        void WriteAllAccounts(List<Account> accounts);
    }
}