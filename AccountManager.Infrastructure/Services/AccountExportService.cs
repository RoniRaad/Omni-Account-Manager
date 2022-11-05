using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;

namespace AccountManager.Infrastructure.Services
{
	public class AccountExportService : IAccountExportService
	{
		private readonly IGeneralFileSystemService _fileSystemService;
		private readonly IAccountService _accountService;
		private readonly IAppState _appState;

		public AccountExportService(IGeneralFileSystemService fileSystemService, IAccountService accountService, IAppState appState)
		{
			_fileSystemService = fileSystemService;
			_accountService = accountService;
			_appState = appState;
		}

		public async Task ExportAccountsAsync(List<string> accountIds, string password, string filePath)
		{
			var accounts = await _accountService.GetAllAccountsMinAsync();
			accounts = accounts.Where(a => accountIds.Contains(a.Id ?? "")).ToList();
			filePath = filePath.EndsWith(".omni") ? filePath : Path.Combine(filePath, ".omni");

			await _fileSystemService.WriteUnmanagedData(accounts, filePath, password);
		}

		public async Task ImportAccountsAsync(string filePath, string password)
		{
			var accounts = await _fileSystemService.ReadUnmanagedData<List<Account>>(filePath, password);
			_appState.Accounts.AddRange(accounts.Where((account) =>
				!_appState.Accounts.Exists((acc) => acc.Id == account.Id)));
		}
	}
}
