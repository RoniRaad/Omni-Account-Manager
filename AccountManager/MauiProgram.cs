using Microsoft.AspNetCore.Components.WebView.Maui;
using AccountManager.Data;
using AccountManager.Infrastructure.Services;
using AccountManager.Core.Services;

namespace AccountManager;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.RegisterBlazorMauiWebView()
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddBlazorWebView();
		builder.Services.AddSingleton<IIOService, IOService>();
		builder.Services.AddSingleton<AuthService>();

		return builder.Build();
	}
}
