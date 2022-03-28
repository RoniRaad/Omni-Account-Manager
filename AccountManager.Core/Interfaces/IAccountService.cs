using AccountManager.Core.Models;

namespace AccountManager.Core.Interfaces
{
    public interface IAccountService
    {
        void AddAccount(Account account);
        void EditAccount(Account editedAccount);
        List<Account> GetAllAccounts();
        void Login(Account account);
        void RemoveAccount(Account account);
        void WriteAllAccounts(List<Account> accounts);
    }
}