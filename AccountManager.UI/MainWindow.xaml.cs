using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using AccountManager.Core.Enums;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Models.AppSettings;
using AccountManager.Core.Models.RiotGames.League;
using AccountManager.Core.Models.RiotGames.League.Requests;
using AccountManager.Core.Models.RiotGames.Valorant;
using AccountManager.Core.Services;
using AccountManager.Core.Services.GraphServices;
using AccountManager.Core.Services.GraphServices.Cached;
using AccountManager.Infrastructure.CachedClients;
using AccountManager.Infrastructure.Clients;
using AccountManager.Infrastructure.Services;
using AccountManager.Infrastructure.Services.FileSystem;
using AccountManager.Infrastructure.Services.Platform;
using AccountManager.Infrastructure.Services.Token;
using AccountManager.UI.Extensions;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using IPC.NamedPipe;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NeoSmart.Caching.Sqlite;
using Plk.Blazor.DragDrop;
using static AccountManager.Core.Services.AppState;
using Squirrel;
using Microsoft.Extensions.Options;

namespace AccountManager.UI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
    {
		public IConfigurationRoot Configuration { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Vulnerability", 
			"S4830:Server certificates should be verified during SSL/TLS connections", Justification = "This is for communicating with a local api.")]
        public MainWindow()
        {

            // This file acts as a flag to delete the cache file before initializing
            if (File.Exists(@$"{IOService.DataPath}\deletecache"))
            {
				File.Delete(@$"{IOService.DataPath}\cache.db");
				File.Delete(@$"{IOService.DataPath}\deletecache");
			}
			var builder = new ConfigurationBuilder()
			.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
			Configuration = builder.Build();

			ServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddBlazorWebView();
            serviceCollection.AddBlazorDragDrop();
            serviceCollection.AddOptions();
            serviceCollection.AddLogging();
			serviceCollection.AddSqliteCache(options => {
				options.CachePath = @$"{IOService.DataPath}\cache.db";
			});
			serviceCollection.AddMemoryCache();
			serviceCollection.AddAutoMapperMappings();
			serviceCollection.AddNamedClients(Configuration);
            serviceCollection.Configure<RiotApiUri>(Configuration.GetSection("RiotApiUri"));
			serviceCollection.Configure<AboutEndpoints>(Configuration.GetSection("AboutEndpoints"));
			serviceCollection.AddSingleton<IIOService, IOService>();
			serviceCollection.AddSingleton<AlertService>();
			serviceCollection.AddState();
            serviceCollection.AddSingleton<AuthService>();
			serviceCollection.AddTransient<LeagueClient>();
			serviceCollection.AddTransient<LeagueTokenClient>();
			serviceCollection.AddSingleton<RiotFileSystemService>();
			serviceCollection.AddSingleton<ValorantClient>();
            serviceCollection.AddSingleton<LeagueFileSystemService>();
            serviceCollection.AddSingleton<ValorantGraphService>();
            serviceCollection.AddSingleton<IAppState, AppState>();
            serviceCollection.AddSingleton<ILeagueClient, LeagueClient>();
            serviceCollection.AddSingleton<ISteamLibraryService, SteamLibraryService>();
            serviceCollection.AddSingleton<IShortcutService, ShortcutService>();
			serviceCollection.AddSingleton<RiotClient>();
			serviceCollection.AddSingleton<LeagueClient>();
            serviceCollection.AddSingleton<ILeagueTokenClient>((services) => new CachedLeagueTokenClient(services.GetRequiredService<IMemoryCache>(), services.GetRequiredService<LeagueTokenClient>()));
            serviceCollection.AddSingleton<IValorantClient>((services) => new CachedValorantClient(services.GetRequiredService<IMemoryCache>(), services.GetRequiredService<IDistributedCache>(), services.GetRequiredService<ValorantClient>()));
            serviceCollection.AddSingleton<IRiotClient>((services) => new CachedRiotClient(services.GetRequiredService<IMemoryCache>(), services.GetRequiredService<RiotClient>()));
			serviceCollection.AddSingleton<ILeagueClient>((services) => new CachedLeagueClient(services.GetRequiredService<IMemoryCache>(), services.GetRequiredService<LeagueClient>()));
			serviceCollection.AddSingleton<ILeagueGraphService>((services) => new CachedLeagueGraphService(services.GetRequiredService<IDistributedCache>(), services.GetRequiredService<LeagueGraphService>()));
            serviceCollection.AddSingleton<IValorantGraphService>((services) => new CachedValorantGraphService(services.GetRequiredService<IDistributedCache>(), services.GetRequiredService<ValorantGraphService>()));
            serviceCollection.AddSingleton<ICurlRequestBuilder, CurlRequestBuilder>();
			serviceCollection.AddSingleton<LeagueGraphService>();
			serviceCollection.AddSingleton<ITeamFightTacticsGraphService, TeamFightTacticsGraphService>();
			serviceCollection.AddSingleton<IIpcService, IpcService>();
			serviceCollection.AddSingleton<ICurlRequestBuilder, CurlRequestBuilder>();
			serviceCollection.AddSingleton<ICurlRequestBuilder, CurlRequestBuilder>();
			serviceCollection.AddSingleton<LeagueTokenService>();
			serviceCollection.AddBlazorise(options =>
			{
				options.Immediate = true;
			})
			.AddBootstrapProviders()
			.AddFontAwesomeIcons();
			serviceCollection.AddSingleton<IAccountService, AccountService>();
			serviceCollection.AddSingleton<IUserSettingsService<UserSettings>, UserSettingsService<UserSettings>>();
			serviceCollection.AddFactory<AccountType, IPlatformService>()
				.AddImplementation<SteamPlatformService>(AccountType.Steam)
				.AddImplementation<LeaguePlatformService>(AccountType.League)
				.AddImplementation<TeamFightTacticsPlatformService>(AccountType.TeamFightTactics)
				.AddImplementation<ValorantPlatformService>(AccountType.Valorant)
				.Build();
			serviceCollection.AddFactory<AccountType, ITokenService>()
				.AddImplementation<LeagueTokenService>(AccountType.League)
				.AddImplementation<LeagueTokenService>(AccountType.TeamFightTactics)
				.AddImplementation<RiotTokenService>(AccountType.Valorant)
				.Build();

			var builtServiceProvider = serviceCollection.BuildServiceProvider();

            Resources.Add("services", builtServiceProvider);
			InitializeComponent();

			var updateUrl = builtServiceProvider.GetRequiredService<IOptions<AboutEndpoints>>().Value;
			if (updateUrl?.Github is not null)
				Task.Run(async () => await CheckForUpdate(updateUrl.Github));

			TrySetVersionNumber();
        }

        private void TrySetVersionNumber()
		{
            try
            {
				var version = Assembly.GetExecutingAssembly().GetName()?.Version?.ToString();
				this.Dispatcher.Invoke(() => {
					versionNum.Text = $"v{version}";
				});
            }
            catch
            {
                this.Dispatcher.Invoke(() => {
                    versionNum.Text = "";
                });
            }
        }

		private async Task CheckForUpdate(string url)
		{
			var manager = await UpdateManager.GitHubUpdateManager(url);
			await manager.UpdateApp();
		}

		private void Close(object sender, RoutedEventArgs e)
        {
			this.Close();
        }

        private void Minimize(object sender, RoutedEventArgs e)
        {
			SystemCommands.MinimizeWindow(this);
		}
        private void Maximize(object sender, RoutedEventArgs e)
        {
			if (this.WindowState != WindowState.Maximized)
				SystemCommands.MaximizeWindow(this);
			else
                SystemCommands.RestoreWindow(this);
        }
    }
}
