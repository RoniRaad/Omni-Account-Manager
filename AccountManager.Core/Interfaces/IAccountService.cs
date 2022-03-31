using AccountManager.Core.Models;

namespace AccountManager.Core.Interfaces
{
    public interface IAccountService
    {
        void AddAccount(Account account);
        void EditAccount(Account editedAccount);
        Task<List<Account>> GetAllAccounts();
        List<Account> GetAllAccountsMin();
        void Login(Account account);
        void RemoveAccount(Account account);
        void WriteAllAccounts(List<Account> accounts);
    }
}