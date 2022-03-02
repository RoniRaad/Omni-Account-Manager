using Microsoft.AspNetCore.Components.WebView.Maui;
using AccountManager.Data;
using AccountManager.Infrastructure.Services;
using AccountManager.Core.Services;
using AccountManager.Core.Enums;
using AccountManager.Core.Interfaces;
using AccountManager.UI.Extensions;
using AccountManager.Core.ViewModels;
using AccountManager.Infrastructure.Services.RankingServices;
using AccountManager.Infrastructure.Clients;

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
		builder.Services.AddSingleton<IRankingService, RiotRankingService>();
		builder.Services.AddSingleton<LeagueClient>();
		builder.Services.AddSingleton<IRiotClient, RiotClient>();
		builder.Services.AddSingleton<LeagueTokenService>();
		builder.Services.AddSingleton<AccountPageViewModel>();
		builder.Services.AddFactory<AccountType, IPlatformService>()
			.AddImplementation<SteamPlatformService>(AccountType.Steam)
			.AddImplementation<LeaguePlatformService>(AccountType.League)
			.AddImplementation<ValorantPlatformService>(AccountType.Valorant)
			.Build();

		var app = builder.Build();
		return app;
	}
}
