using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AccountManager.Core.Enums;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Services;
using AccountManager.Core.ViewModels;
using AccountManager.Infrastructure.Clients;
using AccountManager.Infrastructure.Services;
using AccountManager.Infrastructure.Services.Platform;
using AccountManager.Infrastructure.Services.Token;
using AccountManager.UI.Extensions;
using CloudFlareUtilities;
using Microsoft.Extensions.DependencyInjection;
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
			serviceCollection.AddSingleton<SettingsViewModel>();
			serviceCollection.AddTransient<RemoteLeagueClient>();
			serviceCollection.AddSingleton<LocalLeagueClient>();
			serviceCollection.AddSingleton<ILeagueClient, RemoteLeagueClient>();
			serviceCollection.AddSingleton<IRiotClient, RiotClient>();
			serviceCollection.AddSingleton<LeagueTokenService>();
			serviceCollection.AddSingleton<AccountPageViewModel>();
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
    }
}
