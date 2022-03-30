using System.Net.Http;
using System.Windows;
using AccountManager.Core.Enums;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Services;
using AccountManager.Infrastructure.Clients;
using AccountManager.Infrastructure.Services;
using AccountManager.Infrastructure.Services.FileSystem;
using AccountManager.Infrastructure.Services.Platform;
using AccountManager.Infrastructure.Services.Token;
using AccountManager.UI.Extensions;
using CloudFlareUtilities;
using Microsoft.Extensions.DependencyInjection;
using NeoSmart.Caching.Sqlite;
using Plk.Blazor.DragDrop;

namespace AccountManager.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddBlazorWebView();
            serviceCollection.AddBlazorDragDrop();
            serviceCollection.AddSqliteCache(options => {
				options.CachePath = @".\cache.db";
			});
			serviceCollection.AddMemoryCache();
			serviceCollection.AddHttpClient("CloudflareBypass").ConfigureHttpMessageHandlerBuilder(x =>
			{
				x.PrimaryHandler = new ClearanceHandler
				{
					MaxRetries = 2
				};
			});
			serviceCollection.AddHttpClient("SSLBypass").ConfigureHttpMessageHandlerBuilder(x =>
			{
				var httpClientHandler = new HttpClientHandler();
				httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
				{
					return true;
				};

				x.PrimaryHandler = httpClientHandler;
			});
			serviceCollection.AddSingleton<IIOService, IOService>();
			serviceCollection.AddSingleton<AuthService>();
			serviceCollection.AddSingleton<AlertService>();
			serviceCollection.AddTransient<RemoteLeagueClient>();
			serviceCollection.AddSingleton<LocalLeagueClient>();
			serviceCollection.AddSingleton<RiotLockFileService>();
			serviceCollection.AddSingleton<LeagueLockFileService>();
			serviceCollection.AddSingleton<ILeagueClient, RemoteLeagueClient>();
			serviceCollection.AddSingleton<IRiotClient, RiotClient>();
			serviceCollection.AddSingleton<LeagueTokenService>();
			serviceCollection.AddSingleton<IAccountService, AccountService>();
			serviceCollection.AddSingleton<IUserSettingsService<UserSettings>, UserSettingsService<UserSettings>>();
			serviceCollection.AddFactory<AccountType, IPlatformService>()
				.AddImplementation<SteamPlatformService>(AccountType.Steam)
				.AddImplementation<LeaguePlatformService>(AccountType.League)
				.AddImplementation<TFTPlatformService>(AccountType.TFT)
				.AddImplementation<ValorantPlatformService>(AccountType.Valorant)
				.Build();
			serviceCollection.AddFactory<AccountType, ITokenService>()
				.AddImplementation<LeagueTokenService>(AccountType.League)
				.AddImplementation<LeagueTokenService>(AccountType.TFT)
				.AddImplementation<RiotTokenService>(AccountType.Valorant)
				.Build();
			Resources.Add("services", serviceCollection.BuildServiceProvider());

			InitializeComponent();
        }

		private void Close(object sender, RoutedEventArgs e)
        {
			this.Close();
        }

        private void Minimize(object sender, RoutedEventArgs e)
        {
			SystemCommands.MinimizeWindow(this);
		}
    }
}
