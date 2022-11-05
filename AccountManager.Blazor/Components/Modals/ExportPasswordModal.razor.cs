using Microsoft.AspNetCore.Components;

namespace AccountManager.Blazor.Components.Modals
{
    public partial class ExportPasswordModal
    {
        [Parameter, EditorRequired]
        public ExportAccountRequest ExportAccountRequest { get; set; } = new();
        [Parameter, EditorRequired]
        public Action Close { get; set; } = delegate { };

        public void Submit()
        {
            Close();
        }
    }

    public class ExportAccountRequest
    {
        public List<string> AccountIds { get; set; } = new();
        public string Password { get; set; } = "";
    }
}