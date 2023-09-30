using Microsoft.AspNetCore.Components;
using System.Diagnostics;

namespace AccountManager.Blazor.Pages
{
    public partial class ReloadAccountList
    {
        protected async override Task OnInitializedAsync()
        {
            await Task.Delay(100);
            _navigationManager.NavigateTo("/accountlist");
        }
    }
}