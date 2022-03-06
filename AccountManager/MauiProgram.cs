using Microsoft.AspNetCore.Components.WebView.Maui;
using AccountManager.Infrastructure.Services;
using AccountManager.Core.Services;
using AccountManager.Core.Enums;
using AccountManager.Core.Interfaces;
using AccountManager.UI.Extensions;
using AccountManager.Core.ViewModels;
using AccountManager.Infrastructure.Clients;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using System.Runtime.InteropServices;
using System.Windows;
using AccountManager.Extensions;
using System.Net;
using CloudFlareUtilities;
using Microsoft.Extensions.Http;

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
		builder.Services.AddSingleton<IIOService, IOService>();
		builder.Services.AddSingleton<AuthService>();
		builder.Services.AddTransient<RemoteLeagueClient>();
		builder.Services.AddSingleton<LocalLeagueClient>();
		builder.Services.AddSingleton<ILeagueClient, RemoteLeagueClient>();
		builder.Services.AddSingleton<IRiotClient, RiotClient>();
		builder.Services.AddSingleton<LeagueTokenService>();
		builder.Services.AddSingleton<AccountPageViewModel>();
		builder.Services.AddFactory<AccountType, IPlatformService>()
			.AddImplementation<SteamPlatformService>(AccountType.Steam)
			.AddImplementation<LeaguePlatformService>(AccountType.League)
			.AddImplementation<ValorantPlatformService>(AccountType.Valorant)
			.Build();
		builder.Services.AddFactory<AccountType, ITokenService>()
			.AddImplementation<LeagueTokenService>(AccountType.League)
			.AddImplementation<RiotTokenService>(AccountType.Valorant)
			.Build();
		builder.Services.AddSingleton < Dictionary<AccountType, Dictionary<string, string>>>((x) =>
        {
			var collectionOfRankingColors = new Dictionary<AccountType, Dictionary<string, string>>();
			var leaugeRankingColors = new Dictionary<string, string>()
            {
				{"bronze", "#CD7F32"},
				{"silver", "gray"},
				{"gold", "#FFD700"},
			};
			var valorantRankingColors = new Dictionary<string, string>()
			{
				{"bronze", "#CD7F32"},
				{"silver", "gray"},
				{"gold", "#FFD700"},
			};

			collectionOfRankingColors.Add(AccountType.League, leaugeRankingColors);
			collectionOfRankingColors.Add(AccountType.Valorant, valorantRankingColors);
			return collectionOfRankingColors;
		});

/*new Dictionary<string, string>()
				{
					{"bronze", "#CD7F32"},
					{"silver", "gray"},
					{"gold", "#FFD700"},
				}; */
#if WINDOWS
			builder.ConfigureLifecycleEvents(events => {
						events.AddWindows(wndLifeCycleBuilder => {
							wndLifeCycleBuilder.OnWindowCreated(window => {
								window.SetDimensionsAndCenter(1000, 900);
							});
						});
					});
#endif
	var app = builder.Build();
		return app;
	}
}
