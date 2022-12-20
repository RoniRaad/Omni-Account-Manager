using AccountManager.Core.Models;
using Microsoft.AspNetCore.Components;

namespace AccountManager.Blazor.Components.Modals.ImportExportAccountModal
{
    public partial class ImportAccountsModal
    {
        [Parameter, EditorRequired]
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
                if (!File.Exists(ExportAccountRequest.FilePath))
                {
                    _alertService.AddErrorAlert($"Could not find file {ExportAccountRequest.FilePath}. Please try another file.");
                    return;
                }

                await _exportService.ImportAccountsAsync(ExportAccountRequest.Password,
                                                            ExportAccountRequest.FilePath);
            }
            catch (UnauthorizedAccessException)
            {
                _alertService.AddErrorAlert($"Unable to access directory '{Path.GetDirectoryName(ExportAccountRequest.FilePath)}'. Please choose another one.");
            }
            catch
            {
                _alertService.AddErrorAlert($"Unable to import. Password may have been incorrect!");
            }

            ShowFilePicker = false;

            await Close.InvokeAsync();
        }
    }
}