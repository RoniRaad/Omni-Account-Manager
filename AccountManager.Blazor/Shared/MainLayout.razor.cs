namespace AccountManager.Blazor.Shared
{
    public partial class MainLayout
    {
        public string Password { get; set; } = string.Empty;

        protected override void OnInitialized() => _alertService.UpdateView = () => InvokeAsync(() => StateHasChanged());
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
    }
}