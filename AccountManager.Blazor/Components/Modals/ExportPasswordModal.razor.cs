using AccountManager.Core.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.VisualBasic.FileIO;
using System;

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
            if (string.IsNullOrEmpty(ExportAccountRequest.FolderPath) || string.IsNullOrEmpty(ExportAccountRequest.Password))
                return;

            _exportService.ExportAccountsAsync(ExportAccountRequest.Accounts,
                                               ExportAccountRequest.Password,
                                               Path.Combine(ExportAccountRequest.FolderPath, ExportAccountRequest.Accounts?.First()?.Id));
            Close();
        }
    }

    public class ExportAccountRequest
    {
        public List<Account> Accounts { get; set; } = new();
        public string Password { get; set; } = "";
        public string FolderPath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    }
}