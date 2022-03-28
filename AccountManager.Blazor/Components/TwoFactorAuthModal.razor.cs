using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AccountManager.Core.Services;

namespace AccountManager.Blazor.Components
{
    public partial class TwoFactorAuthModal
    {
        [Parameter, EditorRequired]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public TwoFactorAuthenticationUserRequest Request { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public void Submit()
        {
            Request.Callback(Request.Code);
        }

        public void Close()
        {
            Request.Callback("");
        }
    }
}
