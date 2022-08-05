using AccountManager.Core.Static;
using Microsoft.AspNetCore.Components;

namespace AccountManager.Blazor.Shared
{
    public partial class MainLayout
    {
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; } = false;
        private bool updateAvailable = false;
        public static string RememberMeCacheKey = "rememberPassword";
        public static string PasswordCacheKey = "masterPassword";
        private string filterSidebarStyle = "transform: translate(-188px); width: 0px";
        private bool isFilterSidebarOpen = false;
        private bool settingsModalOpen = false;

        protected override async Task OnInitializedAsync()
        {
            _alertService.Notify += () => InvokeAsync(() => StateHasChanged());

            if (await _persistantCache.GetAsync<bool>(RememberMeCacheKey))
            {
                Password = await _persistantCache.GetAsync<string>(PasswordCacheKey) ?? "";
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

        public async Task Login()
        {
            _authService.Login(Password);
            await InvokeAsync(() => StateHasChanged());
        }

        public async Task Register()
        {
            _authService.Register(Password);
            await InvokeAsync(() => StateHasChanged());
        }

        public async Task RememberMeChanged(ChangeEventArgs e)
        {
            var isChecked = (bool)(e?.Value ?? false);
            await _persistantCache.SetAsync(RememberMeCacheKey, isChecked);
            RememberMe = isChecked;

            if (!isChecked)
                await _persistantCache.RemoveAsync(PasswordCacheKey);
        } 
    }
}