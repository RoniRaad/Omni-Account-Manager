using AccountManager.Core.Models;
using Microsoft.AspNetCore.Components;
using SharpCompress.Common;

namespace AccountManager.Blazor.Components.Modals
{
    public partial class ImportExportAccountModal
    {
        [Parameter]
        public bool IsImport { get; set; } = true;
        [Parameter, EditorRequired]
        public ExportAccountRequest ExportAccountRequest { get; set; } = new();
        [Parameter, EditorRequired]
        public EventCallback Close { get; set; }
        private bool ShowFilePicker = false;
        private string OperationName { get { return IsImport ? "Import" : "Export"; } }
        public async Task Submit()
        {
            if (string.IsNullOrEmpty(ExportAccountRequest.FilePath) || string.IsNullOrEmpty(ExportAccountRequest.Password))
                return;

            try
            {
                if (IsImport)
                {
                    if (!File.Exists(ExportAccountRequest.FilePath))
                    {
                        _alertService.AddErrorAlert($"Could not find file {ExportAccountRequest.FilePath}. Please try another file.");
                        return;
                    }

                    await _exportService.ImportAccountsAsync(ExportAccountRequest.Password,
                                                             ExportAccountRequest.FilePath);
                }
                else
                {
                    await _exportService.ExportAccountsAsync(ExportAccountRequest.Accounts,
                                   ExportAccountRequest.Password,
                                   ExportAccountRequest.FilePath);
                }
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