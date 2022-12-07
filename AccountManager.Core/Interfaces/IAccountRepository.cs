using AccountManager.Core.Models;

namespace AccountManager.Core.Interfaces
{
    public interface IAccountRepository
    {
        Task<Account> Create(Account account);
        Task Delete(Guid id);
        Task<Account?> Get(Guid id);
        Task<List<Account>> GetAll();
        Task<Account> Update(Account account);
    }
}