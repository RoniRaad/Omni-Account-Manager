using System.IO;
using System.Reflection;
using System.Windows;
using AccountManager.Core.Enums;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models.AppSettings;
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
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NeoSmart.Caching.Sqlite;
using Plk.Blazor.DragDrop;
using Squirrel;
using Microsoft.Extensions.Options;
using AccountManager.Core.Models.UserSettings;
using System.Collections.Generic;
using System;
using AccountManager.Core.Models.RiotGames.Valorant;
using Microsoft.Extensions.Logging;
using System.Configuration;
using AccountManager.UI.Services;

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
			// Initialize datapath
			if (!Directory.Exists(IOService.DataPath))
            {
				Directory.CreateDirectory(IOService.DataPath);
            }

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
			serviceCollection.AddLogging(builder =>
			{
                builder.AddConfiguration(Configuration.GetSection("Logging"));
                builder.AddFile(o => o.RootPath = AppContext.BaseDirectory);
            });
            serviceCollection.AddSqliteCache(options => {
				options.CachePath = @$"{IOService.DataPath}\cache.db";
			});
			serviceCollection.AddMemoryCache();
			serviceCollection.AddAutoMapperMappings();
			serviceCollection.AddNamedClients(Configuration);
            serviceCollection.Configure<RiotApiUri>(Configuration.GetSection("RiotApiUri"));
			serviceCollection.Configure<AboutEndpoints>(Configuration.GetSection("AboutEndpoints"));
			serviceCollection.AddSingleton<IIOService, IOService>();
			serviceCollection.AddSingleton<IAlertService, AlertService>();
			serviceCollection.AddSingleton<IAccountFilterService, AccountFilterService>();
			serviceCollection.AddState();
			serviceCollection.AddAuth();
			serviceCollection.AddLogging();
			serviceCollection.AddSingleton<IRiotFileSystemService, RiotFileSystemService>();
            serviceCollection.AddSingleton<LeagueFileSystemService>();
            serviceCollection.AddSingleton<ISteamLibraryService, SteamLibraryService>();
            serviceCollection.AddSingleton<IShortcutService, ShortcutService>();
            serviceCollection.AddSingleton<IAppUpdateService, SquirrelAppUpdateService>();
            serviceCollection.AddSingleton<IEpicGamesExternalAuthService, EpicGamesExternalAuthService>();
            serviceCollection.AddTransient<IEpicGamesTokenClient, EpicGamesTokenClient>();
            serviceCollection.AddTransient<IEpicGamesLibraryService, EpicGamesLibraryService>();

            // Cached Objects
            serviceCollection.AddSingleton<RiotClient>();
			serviceCollection.AddSingleton<LeagueClient>();
            serviceCollection.AddSingleton<ValorantClient>();
            serviceCollection.AddTransient<LeagueClient>();
            serviceCollection.AddTransient<LeagueTokenClient>();
            serviceCollection.AddSingleton<ValorantGraphService>();
            serviceCollection.AddSingleton<LeagueGraphService>();
            serviceCollection.AddSingleton<RiotTokenClient>();

            serviceCollection.AddSingleton<IRiotTokenClient>((services) => new CachedRiotTokenClient(services.GetRequiredService<IMemoryCache>(), services.GetRequiredService<RiotTokenClient>()));
            serviceCollection.AddSingleton<ILeagueTokenClient>((services) => new CachedLeagueTokenClient(services.GetRequiredService<IMemoryCache>(), services.GetRequiredService<LeagueTokenClient>()));
            serviceCollection.AddSingleton<IValorantClient>((services) => new CachedValorantClient(services.GetRequiredService<IMemoryCache>(), services.GetRequiredService<IDistributedCache>(), services.GetRequiredService<ValorantClient>()));
            serviceCollection.AddSingleton<IRiotClient>((services) => new CachedRiotClient(services.GetRequiredService<IMemoryCache>(), services.GetRequiredService<RiotClient>()));
			serviceCollection.AddSingleton<ILeagueClient>((services) => new CachedLeagueClient(services.GetRequiredService<IMemoryCache>(), services.GetRequiredService<LeagueClient>()));
			serviceCollection.AddSingleton<ILeagueGraphService>((services) => new CachedLeagueGraphService(services.GetRequiredService<IDistributedCache>(), services.GetRequiredService<LeagueGraphService>()));
            serviceCollection.AddSingleton<IValorantGraphService>((services) => new CachedValorantGraphService(services.GetRequiredService<IDistributedCache>(), services.GetRequiredService<ValorantGraphService>()));
            serviceCollection.AddSingleton<IHttpRequestBuilder, CurlRequestBuilder>();
			serviceCollection.AddSingleton<ITeamFightTacticsGraphService, TeamFightTacticsGraphService>();
			serviceCollection.AddSingleton<IIpcService, IpcService>();
			serviceCollection.AddSingleton<IHttpRequestBuilder, CurlRequestBuilder>();
			serviceCollection.AddSingleton<IHttpRequestBuilder, CurlRequestBuilder>();
			serviceCollection.AddBlazorise(options =>
			{
				options.Immediate = true;
			})
			.AddBootstrapProviders()
			.AddFontAwesomeIcons();
			serviceCollection.AddSingleton<IAccountService, AccountService>();
			serviceCollection.AddSingleton<IUserSettingsService<GeneralSettings>, UserSettingsService<GeneralSettings>>();
			serviceCollection.AddSingleton<IUserSettingsService<SteamSettings>, UserSettingsService<SteamSettings>>();
			serviceCollection.AddSingleton<IUserSettingsService<LeagueSettings>, UserSettingsService<LeagueSettings>>();
			serviceCollection.AddSingleton<IUserSettingsService<Dictionary<Guid, AccountListItemSettings>>, UserSettingsService<Dictionary<Guid, AccountListItemSettings>>>();
			serviceCollection.AddFactory<AccountType, IPlatformService>()
				.AddImplementation<SteamPlatformService>(AccountType.Steam)
				.AddImplementation<LeaguePlatformService>(AccountType.League)
				.AddImplementation<TeamFightTacticsPlatformService>(AccountType.TeamFightTactics)
				.AddImplementation<ValorantPlatformService>(AccountType.Valorant)
				.AddImplementation<EpicGamesPlatformService>(AccountType.EpicGames)
                .Build();
			serviceCollection.AddFactory<AccountType, ITokenService>()
				.AddImplementation<LeagueTokenService>(AccountType.League)
				.AddImplementation<LeagueTokenService>(AccountType.TeamFightTactics)
				.AddImplementation<RiotTokenService>(AccountType.Valorant)
				.Build();

			var builtServiceProvider = serviceCollection.BuildServiceProvider();

            Resources.Add("services", builtServiceProvider);

			InitializeComponent();
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
