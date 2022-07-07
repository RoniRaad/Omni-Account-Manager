using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using AccountManager.Blazor.Shared;
using AccountManager.Blazor;
using Plk.Blazor.DragDrop;
using AccountManager.Core.Enums;

namespace AccountManager.Blazor.Components.AccountListTile
{
    public partial class TileTopBar
    {
        [Parameter]
        public string Title { get; set; } = "";

        [Parameter]
        public AccountType AccountType { get; set; }

        [Parameter]
        public EventCallback MouseEnterDragLogo { get; set; }

        [Parameter]
        public EventCallback MouseExitDragLogo { get; set; }
    }
}