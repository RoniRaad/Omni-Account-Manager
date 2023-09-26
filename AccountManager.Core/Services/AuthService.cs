using AccountManager.Core.Interfaces;
using AccountManager.Core.Static;
using Microsoft.Extensions.Caching.Distributed;

namespace AccountManager.Core.Services
{
    public sealed class SqliteAuthService : IAuthService
    {
        private readonly IAccountEncryptedRepository _accountRepository;
        private readonly IAlertService _alertService;
        private readonly IDistributedCache _persistantCache;
        public string PasswordHash { get; set; } = "";
        public bool LoggedIn { get; set; }
        public bool AuthInitialized { get; set; }
        public SqliteAuthService(IAccountEncryptedRepository accountRepository, IAlertService alertService,
            IDistributedCache persistantCache, IGeneralFileSystemService fileSystemService)
        {
            _accountRepository = accountRepository;
            _alertService = alertService;
            _persistantCache = persistantCache;
            AuthInitialized = fileSystemService.ValidateData();
        }

        public async Task<bool> LoginAsync(string password)
        {
            PasswordHash = StringEncryption.Hash(password);
            LoggedIn = _accountRepository.TryDecrypt(PasswordHash);
            if (!LoggedIn)
            {
                _alertService.AddErrorAlert("Error incorrect password!");
            }

            if (await _persistantCache.GetAsync<bool>(CacheKeys.LoginCacheKeys.RememberMe))
                await _persistantCache.SetAsync(CacheKeys.LoginCacheKeys.RememberedPassword, password);

            return LoggedIn;
        }

        public async Task<bool> RegisterAsync(string password)
        {
            PasswordHash = StringEncryption.Hash(password);
            LoggedIn = true;

			return LoggedIn;
		}

		public async Task ChangePasswordAsync(string oldPassword, string newPassword)
        {
            oldPassword = StringEncryption.Hash(oldPassword);
            newPassword = StringEncryption.Hash(newPassword);

            if (string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword) || !_accountRepository.TryDecrypt(oldPassword))
                return;

            if (_accountRepository.TryChangePassword(oldPassword, newPassword))
                PasswordHash = newPassword;
        }
    }
}
