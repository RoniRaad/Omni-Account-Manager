using AccountManager.Core.Models;
using Microsoft.AspNetCore.Components;

namespace AccountManager.Blazor.Components.Modals.ImportExportAccountModal
{
    public partial class ExportAccountsModal
    {
        [Parameter]
        public ExportAccountRequest ExportAccountRequest { get; set; } = new();
        [Parameter, EditorRequired]
        public EventCallback Close { get; set; }
        private bool ShowFilePicker = false;

        public async Task Submit()
        {
            if (string.IsNullOrEmpty(ExportAccountRequest.FilePath) || string.IsNullOrEmpty(ExportAccountRequest.Password))
                return;

            try
            {
                await _exportService.ExportAccountsAsync(ExportAccountRequest.Accounts,
                                ExportAccountRequest.Password,
                                ExportAccountRequest.FilePath);
            }
            catch (UnauthorizedAccessException)
            {
                _alertService.AddErrorAlert($"Unable to access directory '{Path.GetDirectoryName(ExportAccountRequest.FilePath)}'. Please choose another one.");
            }
            catch
            {
                _alertService.AddErrorAlert($"Unable to export. Password may have been incorrect!");
            }

            ShowFilePicker = false;

            await Close.InvokeAsync();
        }

        private void RemoveAccountFromRequest(Account account)
        {
            ExportAccountRequest.Accounts.RemoveAll((item) => item.Id == account.Id);
        }

        private void AddAccountFromRequest(Account account)
        {
            ExportAccountRequest.Accounts.Add(account);
        }
    }
}