using AccountManager.Core.Models;
using Microsoft.AspNetCore.Components;

namespace AccountManager.Blazor.Components.Modals
{
    public partial class ExportPasswordModal
    {
        [Parameter, EditorRequired]
        public ExportAccountRequest ExportAccountRequest { get; set; } = new();
        [Parameter, EditorRequired]
        public Action Close { get; set; } = delegate { };
        private bool ShowFilePicker = false;

        public void Submit()
        {
            _exportService.ExportAccountsAsync(ExportAccountRequest.Accounts,
                                               ExportAccountRequest.Password,
                                               Path.Combine(ExportAccountRequest.FilePath, ExportAccountRequest.Accounts?.First()?.Id));
            Close();
        }
    }

    public class ExportAccountRequest
    {
        public List<Account> Accounts { get; set; } = new();
        public string Password { get; set; } = "";
        public string FilePath { get; set; } = "";
    }
}