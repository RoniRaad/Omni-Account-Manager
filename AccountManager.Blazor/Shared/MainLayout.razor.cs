using AccountManager.Core.Static;

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
    }
}