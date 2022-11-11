using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;

namespace AccountManager.Infrastructure.Services
{
	public class AccountExportService : IAccountExportService
	{
		private readonly IGeneralFileSystemService _fileSystemService;
		private readonly IAppState _appState;
		private readonly IAlertService _alertService;

		public AccountExportService(IGeneralFileSystemService fileSystemService, IAppState appState, IAlertService alertService)
		{
			_fileSystemService = fileSystemService;
			_appState = appState;
			_alertService = alertService;
		}

		public async Task ExportAccountsAsync(List<Account> accounts, string password, string filePath)
		{
			try
			{
                filePath = Path.ChangeExtension(filePath, ".omni");
                await _fileSystemService.WriteUnmanagedData(accounts, filePath, password);
            }
			catch (UnauthorizedAccessException)
			{
				_alertService.AddErrorAlert($"Unable to access directory '{Path.GetDirectoryName(filePath)}'. Please choose another one.");
			}
		}

		public async Task ImportAccountsAsync(string filePath, string password)
		{
			var accounts = await _fileSystemService.ReadUnmanagedData<List<Account>>(filePath, password);
			_appState.Accounts.AddRange(accounts.Where((account) =>
				!_appState.Accounts.Exists((acc) => acc.Id == account.Id)));

			_appState.SaveAccounts();

        }
	}
}
