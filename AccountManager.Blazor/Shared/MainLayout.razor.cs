using AccountManager.Core.Static;
using Microsoft.AspNetCore.Components;

namespace AccountManager.Blazor.Shared
{
    public partial class MainLayout
    {
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; } = false;
        private bool updateAvailable = false;
        private string filterSidebarStyle = "transform: translate(-188px); width: 0px";
        private bool isFilterSidebarOpen = false;
        private bool settingsModalOpen = false;

        protected override async Task OnInitializedAsync()
        {
            _alertService.Notify += () => InvokeAsync(() => StateHasChanged());

            if (await _persistantCache.GetAsync<bool>(CacheKeys.LoginCacheKeys.RememberMe))
            {
                Password = await _persistantCache.GetAsync<string>(CacheKeys.LoginCacheKeys.RememberedPassword) ?? "";
                RememberMe = true;
            }

            updateAvailable = await _appUpdateService.CheckForUpdate();
        }

        private void ToggleFilterSidebar()
        {
            isFilterSidebarOpen = !isFilterSidebarOpen;

            if (isFilterSidebarOpen)
                filterSidebarStyle = "transform: translate(0); animation: popout-translate .3s ease; width: 188px";
            else
                filterSidebarStyle = "transform: translate(-188px); animation: popin-translate .3s ease; width: 0px";
        }

        public async Task LoginAsync()
        {
            await _authService.LoginAsync(Password);
            await InvokeAsync(() => StateHasChanged());
        }

        public async Task RegisterAsync()
        {
            await _authService.RegisterAsync(Password);
            await InvokeAsync(() => StateHasChanged());
        }

        public async Task RememberMeChanged(ChangeEventArgs e)
        {
            var isChecked = (bool)(e?.Value ?? false);
            await _persistantCache.SetAsync(CacheKeys.LoginCacheKeys.RememberMe, isChecked);
            RememberMe = isChecked;

            if (!isChecked)
                await _persistantCache.RemoveAsync(CacheKeys.LoginCacheKeys.RememberedPassword);
        } 
    }
}