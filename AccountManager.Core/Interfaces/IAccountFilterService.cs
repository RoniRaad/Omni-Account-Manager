using AccountManager.Core.Enums;

namespace AccountManager.Core.Interfaces
{
	public interface IAccountFilterService
	{
		string AccountNameFilter { get; set; }
		List<AccountType> AccountTypeFilter { get; set; }

        event Action OnFilterChanged;

        Task Save();
	}
}