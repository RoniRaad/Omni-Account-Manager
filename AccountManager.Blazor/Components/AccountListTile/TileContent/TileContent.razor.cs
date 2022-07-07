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
using AccountManager.Blazor.Components.AccountListTile.TileContent.Pages;
using AccountManager.Core.Models;

namespace AccountManager.Blazor.Components.AccountListTile.TileContent
{
    public partial class TileContent
    {
        [Parameter]
        public Account Account { get; set; } = new();

        [Parameter]
        public EventCallback MouseEnterGraph { get; set; }

        [Parameter]
        public EventCallback MouseExitGraph { get; set; }
    }
}