using AccountManager.Core.Models;

namespace AccountManager.Core.Interfaces
{
    public interface IAccountEncryptedRepository
    {
        Task<Account> Create(Account account, string password);
        Task Delete(Guid id, string password);
        Task<Account?> Get(Guid id, string password);
        Task<List<Account>> GetAll(string password);
        bool TryChangePassword(string oldPassword, string newPassword);
        bool TryDecrypt(string password);
        Task<Account> Update(Account account, string password);
    }
}