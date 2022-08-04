using Microsoft.Extensions.Caching.Distributed;
using AccountManager.Core.Static;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Enums;

namespace AccountManager.Core.Services
{
	public class AccountFilterService : IAccountFilterService
	{
		private readonly IDistributedCache _persistantCache;
		public event Action OnFilterChanged = delegate { };
		public string AccountNameFilter { get; set; } = string.Empty;
		public List<AccountType> AccountTypeFilter { get; set; } = Enum.GetValues<AccountType>().ToList();
		public AccountFilterService(IDistributedCache persistantCache)
		{
			_persistantCache = persistantCache;
			Task.Run(SetValuesFromCache);
		}

		private async Task SetValuesFromCache()
		{
			AccountNameFilter = await _persistantCache.GetStringAsync(nameof(AccountNameFilter)) ?? string.Empty;
			AccountTypeFilter = await _persistantCache.GetAsync<List<AccountType>>(nameof(AccountTypeFilter)) ?? Enum.GetValues<AccountType>().ToList();
		}

		public async Task Save()
		{
			await _persistantCache.SetStringAsync(nameof(AccountNameFilter), AccountNameFilter);
			await _persistantCache.SetAsync(nameof(AccountTypeFilter), AccountTypeFilter);
			OnFilterChanged.Invoke();
        }
	}
}
