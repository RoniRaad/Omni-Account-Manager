using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Static;
using Microsoft.Extensions.Caching.Memory;

namespace AccountManager.Infrastructure.CachedRepositories
{
    public class CachedAccountRepository : IAccountEncryptedRepository
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IAccountEncryptedRepository _repo;

        public CachedAccountRepository(IMemoryCache memoryCache, IAccountEncryptedRepository repo)
        {
            _memoryCache = memoryCache;
            _repo = repo;
        }

        public async Task<Account?> Get(Guid id, string password)
        {
            return await _memoryCache.GetOrCreateAsync($"{nameof(CachedAccountRepository)}.{nameof(Get)}.{id}", async (entry) =>
            {
                return await _repo.Get(id, password);
            });
        }

        public async Task<List<Account>> GetAll(string password)
        {
            return await _memoryCache.GetOrCreateAsync($"{nameof(CachedAccountRepository)}.{nameof(GetAll)}", async (entry) =>
            {
                return await _repo.GetAll(password);
            }) ?? new();
        }

        public async Task<Account> Create(Account account, string password)
        {
            var newAccount = await _repo.Create(account, password);
            _memoryCache.Remove($"{nameof(CachedAccountRepository)}.{nameof(Get)}.{newAccount.Id}");
            _memoryCache.Remove($"{nameof(CachedAccountRepository)}.{nameof(GetAll)}");

            return newAccount;
        }

        public async Task<Account> Update(Account account, string password)
        {
            var updateAccount = await _repo.Update(account, password);
            _memoryCache.Remove($"{nameof(CachedAccountRepository)}.{nameof(Get)}.{updateAccount.Id}");
            _memoryCache.Remove($"{nameof(CachedAccountRepository)}.{nameof(GetAll)}");

            return updateAccount;
        }

        public async Task Delete(Guid id, string password)
        {
            await _repo.Delete(id, password);
            _memoryCache.Remove($"{nameof(CachedAccountRepository)}.{nameof(Get)}.{id}");
            _memoryCache.Remove($"{nameof(CachedAccountRepository)}.{nameof(GetAll)}");
        }

        public bool TryDecrypt(string password)
        {

            return _repo.TryDecrypt(password);
        }

        public bool TryChangePassword(string oldPassword, string newPassword)
        {
            return _repo.TryChangePassword(oldPassword, newPassword);
        }
    }
}
