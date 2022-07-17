using AccountManager.Core.Models;
using Microsoft.AspNetCore.Components;

namespace AccountManager.Blazor.Components.Modals.ExportAccountModal
{
    public partial class ExportAccountModal
    {
        [Parameter, EditorRequired]
        public Account Account { get; set; } = new();
        [Parameter, EditorRequired]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Action Close { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private ExportAccountModalPage currentPage = 0;
        public void AddAccount()
        {
            Close();
        }

        public enum ExportAccountModalPage
        {
            CreateDesktopShortcut,
        }
    }
}
