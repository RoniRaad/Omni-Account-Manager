using AccountManager.Core.Enums;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;

namespace AccountManager.Core.Services
{
    public sealed class AccountService : IAccountService
    {
        private readonly IGenericFactory<AccountType, IPlatformService> _platformServiceFactory;
        private readonly IAccountRepository _accountRepository;
        public event Action OnAccountListChanged = delegate { };
        public AccountService(IGenericFactory<AccountType, IPlatformService> platformServiceFactory, IAccountRepository accountRepository)
        {
            _platformServiceFactory = platformServiceFactory;
            _accountRepository = accountRepository;
        }

        public async Task RemoveAccountAsync(Account account)
        {
            var accounts = await GetAllAccountsMinAsync();
            accounts.RemoveAll((acc) => acc?.Id == account.Id);
            await WriteAllAccountsAsync(accounts);
            OnAccountListChanged.Invoke();
        }

        public async Task<List<Account>> GetAllAccountsAsync()
        {
            List<Task> accountTasks = new();
            var accounts = await GetAllAccountsMinAsync();

            var accountsCount = accounts.Count;
            for (int i = 0; i < accountsCount; i++)
            {
                var account = accounts[i];
                var platformService = _platformServiceFactory.CreateImplementation(account.AccountType);

                if (string.IsNullOrEmpty(account.PlatformId))
                    accountTasks.Add(Task.Run(async () => account.PlatformId = (await platformService.TryFetchId(account)).Item2));
            }

            await Task.WhenAll(accountTasks);

            return accounts;
        }

        public async Task<List<Account>> GetAllAccountsMinAsync()
        {
            var accounts = await _accountRepository.GetAll();
            return accounts;
        }

        public async Task LoginAsync(Account account)
        {
            var platformService = _platformServiceFactory.CreateImplementation(account.AccountType);
            await platformService.Login(account);
        }

        public async Task WriteAllAccountsAsync(List<Account> accounts)
        {
            foreach (var account in accounts)
            {
                var oldAccount = _accountRepository.Get(account.Id);
                if (oldAccount is null)
                {
                    await _accountRepository.Create(account);
                }
                else
                {
                    await _accountRepository.Update(account);
                }
            }
        }
    }
}
