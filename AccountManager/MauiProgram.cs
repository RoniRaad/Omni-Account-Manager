using Microsoft.AspNetCore.Components.WebView.Maui;
using AccountManager.Infrastructure.Services;
using AccountManager.Core.Services;
using AccountManager.Core.Enums;
using AccountManager.Core.Interfaces;
using AccountManager.UI.Extensions;
using AccountManager.Core.ViewModels;
using AccountManager.Infrastructure.Clients;
using CloudFlareUtilities;
using AccountManager.Core.Models;
using AccountManager.Infrastructure.Services.Token;
using AccountManager.Infrastructure.Services.Platform;
using AccountManager.Extensions;
using Microsoft.Maui.LifecycleEvents;
using AccountManager.Pages;

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
		builder.Services.AddMemoryCache();
        builder.Services.AddHttpClient("CloudflareBypass").ConfigureHttpMessageHandlerBuilder(x =>
        {
			x.PrimaryHandler = new ClearanceHandler
			{
				MaxRetries = 2
			};
        });
		builder.Services.AddHttpClient("SSLBypass").ConfigureHttpMessageHandlerBuilder(x =>
		{
			var httpClientHandler = new HttpClientHandler();
			httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
			{
				return true;
			};

			x.PrimaryHandler = httpClientHandler;
		});
		builder.Services.AddSingleton<IIOService, IOService>();
		builder.Services.AddSingleton<AuthService>();
		builder.Services.AddSingleton<AlertService>();
		builder.Services.AddSingleton<SettingsViewModel>();
		builder.Services.AddTransient<RemoteLeagueClient>();
		builder.Services.AddSingleton<LocalLeagueClient>();
		builder.Services.AddSingleton<ILeagueClient, RemoteLeagueClient>();
		builder.Services.AddSingleton<IRiotClient, RiotClient>();
		builder.Services.AddSingleton<LeagueTokenService>();
		builder.Services.AddSingleton<AccountPageViewModel>();
		builder.Services.AddSingleton<IUserSettingsService<UserSettings>, UserSettingsService<UserSettings>>();
		builder.Services.AddFactory<AccountType, IPlatformService>()
			.AddImplementation<SteamPlatformService>(AccountType.Steam)
			.AddImplementation<LeaguePlatformService>(AccountType.League)
			.AddImplementation<TFTPlatformService>(AccountType.TFT)
			.AddImplementation<ValorantPlatformService>(AccountType.Valorant)
			.Build();
		builder.Services.AddFactory<AccountType, ITokenService>()
			.AddImplementation<LeagueTokenService>(AccountType.League)
			.AddImplementation<LeagueTokenService>(AccountType.TFT)
			.AddImplementation<RiotTokenService>(AccountType.Valorant)
			.Build();

		#if WINDOWS
			builder.ConfigureLifecycleEvents(events => {
				events.AddWindows(wndLifeCycleBuilder => {
					wndLifeCycleBuilder.OnWindowCreated(window => {
						window.SetDimensionsAndCenter(1200, 900);
					});
				});
			});
		#endif
		var app = builder.Build();
		return app;
	}
}
