using AccountManager.Core.Models;
using Microsoft.AspNetCore.Components;

namespace AccountManager.Blazor.Components.Modals
{
    public partial class ConfirmationPrompt
    {
        [Parameter, EditorRequired]
        public ConfirmationRequest Request { get; set; } = new() { Callback = delegate(bool _) { } };
    } 
}