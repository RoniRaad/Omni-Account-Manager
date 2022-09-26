using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AccountManager.Core.Services;

namespace AccountManager.Blazor.Components.Modals
{
    public partial class TwoFactorAuthModal
    {
        [Parameter, EditorRequired]
        public TwoFactorAuthenticationUserRequest Request { get; set; } = new();

        public void Submit()
        {
            Request?.Callback(Request.Code);
        }

        public void Close()
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            Request.Callback("");
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }
    }
}
