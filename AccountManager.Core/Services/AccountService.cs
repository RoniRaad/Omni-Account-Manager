using AccountManager.Core.Enums;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;

namespace AccountManager.Core.Services
{
    public sealed class AccountService : IAccountService
    {
        private readonly IGenericFactory<AccountType, IPlatformService> _platformServiceFactory;
        private readonly IAccountEncryptedRepository _accountRepository;
        private readonly IAuthService _authService;
        public event Action OnAccountListChanged = delegate { };
        public AccountService(IGenericFactory<AccountType, IPlatformService> platformServiceFactory, 
            IAccountEncryptedRepository accountRepository, IAuthService authService)
        {
            _platformServiceFactory = platformServiceFactory;
            _accountRepository = accountRepository;
            _authService = authService;
        }

        public async Task DeleteAccountAsync(Account account)
        {
            await _accountRepository.Delete(account.Id, _authService.PasswordHash);
            OnAccountListChanged.Invoke();
        }

        public async Task SaveAccountAsync(Account account)
        {
            var currentAccount = await _accountRepository.Get(account.Id, _authService.PasswordHash);

            if (currentAccount is null)
            {
                await _accountRepository.Create(account, _authService.PasswordHash);
            }
            else
            {
                await _accountRepository.Update(account, _authService.PasswordHash);
            }

            OnAccountListChanged.Invoke();
        }

        public async Task<Account?> GetAccountAsync(Guid id)
        {
            return await _accountRepository.Get(id, _authService.PasswordHash);
        }

        public async Task<List<Account>> GetAllAccountsAsync()
        {
            bool platformIdUpdated = false;
            List<Task> accountTasks = new();
            var accounts = await _accountRepository.GetAll(_authService.PasswordHash);

            var accountsCount = accounts.Count;
            for (int i = 0; i < accountsCount; i++)
            {
                var account = accounts[i];
                var platformService = _platformServiceFactory.CreateImplementation(account.AccountType);

                if (string.IsNullOrEmpty(account.PlatformId))
                {
                    accountTasks.Add(Task.Run(async () =>
                    {
                        account.PlatformId = (await platformService.TryFetchId(account)).Item2;
                        await SaveAccountAsync(account);
                        platformIdUpdated = true;
                    }));
                }
            }

            _ = Task.Run(async () =>
            {
                await Task.WhenAll(accountTasks);
                if (platformIdUpdated)
                    OnAccountListChanged.Invoke();
            });

            return accounts;
        }

        public async Task LoginAsync(Account account)
        {
            var platformService = _platformServiceFactory.CreateImplementation(account.AccountType);
            await platformService.Login(account);
        }
    }
}
