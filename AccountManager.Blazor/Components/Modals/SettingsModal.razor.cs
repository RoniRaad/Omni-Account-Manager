using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using Microsoft.AspNetCore.Components;

namespace AccountManager.Blazor.Components.Modals
{
    public partial class SettingsModal
    {
        [Parameter]
        public Action Close { get; set; } = delegate { };
    
    }
}
