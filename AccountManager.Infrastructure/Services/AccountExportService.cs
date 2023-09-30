using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using System.Linq.Expressions;

namespace AccountManager.Infrastructure.Services
{
	public class AccountExportService : IAccountExportService
	{
		private readonly IGeneralFileSystemService _fileSystemService;
		private readonly IAccountService _accountService;
        private readonly IAppState _appState;

		public AccountExportService(IGeneralFileSystemService fileSystemService, IAppState appState, IAccountService accountService)
		{
			_fileSystemService = fileSystemService;
			_appState = appState;
			_accountService = accountService;
		}

		public async Task ExportAccountsAsync(List<Account> accounts, string password, string filePath)
		{
            filePath = Path.ChangeExtension(filePath, ".omni");
            await _fileSystemService.WriteUnmanagedData(accounts, filePath, password);
		}

		public async Task ImportAccountsAsync(string password, string filePath)
		{
			var accounts = await _fileSystemService.ReadUnmanagedData<List<Account>>(filePath, password);
			_appState.Accounts ??= new();
            _appState.Accounts.AddRange(accounts.Where((account) =>
				!_appState.Accounts.Exists((acc) => acc.Id == account.Id)));

			foreach (var account in accounts)
			{
				await _accountService.SaveAccountAsync(account);
			}
        }
	}
}
