using AccountManager.Core.Static;
using Microsoft.AspNetCore.Components;

namespace AccountManager.Blazor.Pages
{
    public partial class LoginPage
    {
		public string Password { get; set; } = string.Empty;
		public bool RememberMe { get; set; } = false;

		protected override async Task OnInitializedAsync()
		{
			if (await _persistantCache.GetAsync<bool>(CacheKeys.LoginCacheKeys.RememberMe))
			{
				Password = await _persistantCache.GetAsync<string>(CacheKeys.LoginCacheKeys.RememberedPassword) ?? "";
				RememberMe = true;
			}

			await base.OnInitializedAsync();
		}
		public async Task LoginAsync()
		{
			if (await _authService.LoginAsync(Password))
				_navManager.NavigateTo("/accountlist", true);
		}

		public async Task RegisterAsync()
		{
			if (await _authService.RegisterAsync(Password))
				_navManager.NavigateTo("/accountlist", true);
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