using AccountManager.Core.Models;

namespace AccountManager.Core.Interfaces
{
    public interface IAccountService
    {
		event Action OnAccountListChanged;

		void AddAccount(Account account);
        void EditAccount(Account editedAccount);
        Task<List<Account>> GetAllAccounts();
        List<Account> GetAllAccountsMin();
        Task Login(Account account);
        void RemoveAccount(Account account);
        void WriteAllAccounts(List<Account> accounts);
    }
}