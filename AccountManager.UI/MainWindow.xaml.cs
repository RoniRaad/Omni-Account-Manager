using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Windows;
using AccountManager.Core.Enums;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Models.AppSettings;
using AccountManager.Core.Models.RiotGames.League;
using AccountManager.Core.Models.RiotGames.Valorant;
using AccountManager.Core.Services;
using AccountManager.Infrastructure.Clients;
using AccountManager.Infrastructure.Services;
using AccountManager.Infrastructure.Services.FileSystem;
using AccountManager.Infrastructure.Services.Platform;
using AccountManager.Infrastructure.Services.Token;
using AccountManager.UI.Extensions;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using CloudFlareUtilities;
using Microsoft.Extensions.Configuration;
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
		public IConfigurationRoot Configuration { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Vulnerability", 
			"S4830:Server certificates should be verified during SSL/TLS connections", Justification = "This is for communicating with a local api.")]
        public MainWindow()
        {
			// This file acts as a flag to delete the cache file before initializing
			if (File.Exists(@".\deletecache"))
            {
				File.Delete(@".\cache.db");
				File.Delete(@".\deletecache");
			}
			var builder = new ConfigurationBuilder()
			.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
			Configuration = builder.Build();

			ServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddBlazorWebView();
            serviceCollection.AddBlazorDragDrop();
            serviceCollection.AddOptions();
			serviceCollection.AddSqliteCache(options => {
				options.CachePath = @".\cache.db";
			});
			serviceCollection.AddMemoryCache();
			var riotApiUri = Configuration.GetSection("RiotApiUri").Get<RiotApiUri>();
			serviceCollection.AddHttpClient("RiotAuth", (httpClient) =>
            {
				httpClient.BaseAddress = new Uri(riotApiUri?.Auth ?? "");
				httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Riot-ClientPlatform", "ew0KCSJwbGF0Zm9ybVR5cGUiOiAiUEMiLA0KCSJwbGF0Zm9ybU9TIjogIldpbmRvd3MiLA0KCSJwbGF0Zm9ybU9TVmVyc2lvbiI6ICIxMC4wLjE5MDQyLjEuMjU2LjY0Yml0IiwNCgkicGxhdGZvcm1DaGlwc2V0IjogIlVua25vd24iDQp9");
				httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("RiotClient/50.0.0.4396195.4381201 rso-auth (Windows;10;;Professional, x64)");
				httpClient.DefaultRequestVersion = HttpVersion.Version20;
			}).ConfigureHttpMessageHandlerBuilder(x =>
			{
				x.PrimaryHandler = new HttpClientHandler
				{
					UseCookies = false,
				};
			});
			serviceCollection.AddHttpClient("RiotEntitlement", (httpClient) =>
			{
				httpClient.BaseAddress = new Uri(riotApiUri?.Entitlement ?? "");
				httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Riot-ClientPlatform", "ew0KCSJwbGF0Zm9ybVR5cGUiOiAiUEMiLA0KCSJwbGF0Zm9ybU9TIjogIldpbmRvd3MiLA0KCSJwbGF0Zm9ybU9TVmVyc2lvbiI6ICIxMC4wLjE5MDQyLjEuMjU2LjY0Yml0IiwNCgkicGxhdGZvcm1DaGlwc2V0IjogIlVua25vd24iDQp9");
				httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("RiotClient/50.0.0.4396195.4381201 rso-auth (Windows;10;;Professional, x64)");
				httpClient.DefaultRequestVersion = HttpVersion.Version20;
			}).ConfigureHttpMessageHandlerBuilder(x =>
			{
				x.PrimaryHandler = new HttpClientHandler
				{
					UseCookies = false
				};
			});
			serviceCollection.AddHttpClient("RiotSessionNA", (httpClient) =>
			{
				httpClient.BaseAddress = new Uri(riotApiUri?.LeagueSessionUS ?? "");
				httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Riot-ClientPlatform", "ew0KCSJwbGF0Zm9ybVR5cGUiOiAiUEMiLA0KCSJwbGF0Zm9ybU9TIjogIldpbmRvd3MiLA0KCSJwbGF0Zm9ybU9TVmVyc2lvbiI6ICIxMC4wLjE5MDQyLjEuMjU2LjY0Yml0IiwNCgkicGxhdGZvcm1DaGlwc2V0IjogIlVua25vd24iDQp9");
				httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("RiotClient/50.0.0.4396195.4381201 rso-auth (Windows;10;;Professional, x64)");
				httpClient.DefaultRequestVersion = HttpVersion.Version20;
			}).ConfigureHttpMessageHandlerBuilder(x =>
			{
				x.PrimaryHandler = new HttpClientHandler
				{
					UseCookies = false
				};
			});
			serviceCollection.AddHttpClient("LeagueNA", (httpClient) =>
			{
				httpClient.BaseAddress = new Uri(riotApiUri?.LeagueNA ?? "");
				httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Riot-ClientPlatform", "ew0KCSJwbGF0Zm9ybVR5cGUiOiAiUEMiLA0KCSJwbGF0Zm9ybU9TIjogIldpbmRvd3MiLA0KCSJwbGF0Zm9ybU9TVmVyc2lvbiI6ICIxMC4wLjE5MDQyLjEuMjU2LjY0Yml0IiwNCgkicGxhdGZvcm1DaGlwc2V0IjogIlVua25vd24iDQp9");
				httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("RiotClient/50.0.0.4396195.4381201 rso-auth (Windows;10;;Professional, x64)");
				httpClient.DefaultRequestVersion = HttpVersion.Version20;
			}).ConfigureHttpMessageHandlerBuilder(x =>
			{
				x.PrimaryHandler = new HttpClientHandler
				{
					UseCookies = false
				};
			});
			serviceCollection.AddHttpClient("Valorant", (httpClient) =>
			{
				httpClient.BaseAddress = new Uri(riotApiUri?.Valorant ?? "");
				httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Riot-ClientPlatform", "ew0KCSJwbGF0Zm9ybVR5cGUiOiAiUEMiLA0KCSJwbGF0Zm9ybU9TIjogIldpbmRvd3MiLA0KCSJwbGF0Zm9ybU9TVmVyc2lvbiI6ICIxMC4wLjE5MDQyLjEuMjU2LjY0Yml0IiwNCgkicGxhdGZvcm1DaGlwc2V0IjogIlVua25vd24iDQp9");
				httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("RiotClient/50.0.0.4396195.4381201 rso-auth (Windows;10;;Professional, x64)");
				httpClient.DefaultRequestVersion = HttpVersion.Version20;
			}).ConfigureHttpMessageHandlerBuilder(x =>
			{
				x.PrimaryHandler = new HttpClientHandler
				{
					UseCookies = false
				};
			});
			serviceCollection.AddHttpClient("ValorantNA", (httpClient) =>
			{
				httpClient.BaseAddress = new Uri(riotApiUri?.ValorantNA ?? "");
				httpClient.DefaultRequestHeaders.TryAddWithoutValidation("X-Riot-ClientPlatform", "ew0KCSJwbGF0Zm9ybVR5cGUiOiAiUEMiLA0KCSJwbGF0Zm9ybU9TIjogIldpbmRvd3MiLA0KCSJwbGF0Zm9ybU9TVmVyc2lvbiI6ICIxMC4wLjE5MDQyLjEuMjU2LjY0Yml0IiwNCgkicGxhdGZvcm1DaGlwc2V0IjogIlVua25vd24iDQp9");
				httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("RiotClient/50.0.0.4396195.4381201 rso-auth (Windows;10;;Professional, x64)");
				httpClient.DefaultRequestVersion = HttpVersion.Version20;
			}).ConfigureHttpMessageHandlerBuilder(x =>
			{
				x.PrimaryHandler = new HttpClientHandler
				{
					UseCookies = false
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

			serviceCollection.Configure<RiotApiUri>(Configuration.GetSection("RiotApiUri"));
			serviceCollection.Configure<AboutEndpoints>(Configuration.GetSection("AboutEndpoints"));
			serviceCollection.AddSingleton<IIOService, IOService>();
			serviceCollection.AddSingleton<AlertService>();
			serviceCollection.AddSingleton<AppState>();
			serviceCollection.AddSingleton<AuthService>();
			serviceCollection.AddAutoMapper((cfg) =>
			{
				cfg.CreateMap<int, ValorantRank>()
				.ForMember(d => d.Tier, opt => opt.MapFrom((src) => ValorantRank.RankMap[src / 3]))
				.ForMember(d => d.Ranking, opt => opt.MapFrom((src) => new string('I', src % 3 + 1)))
				.ForMember(d => d.HexColor, opt => opt.MapFrom((src) => ValorantRank.RankedColorMap[ValorantRank.RankMap[src / 3].ToLower()]));

				cfg.CreateMap<Rank, LeagueRank>()
				.ForMember(d => d.Tier, opt => opt.MapFrom((src) => src.Tier))
				.ForMember(d => d.Ranking, opt => opt.MapFrom((src) => src.Ranking))
				.ForMember(d => d.HexColor, opt => opt.MapFrom((src) => LeagueRank.RankedColorMap[!string.IsNullOrEmpty(src.Tier) ? src.Tier.ToLower() : "unranked"]));

				cfg.CreateMap<Rank, TeamFightTacticsRank>()
				.ForMember(d => d.Tier, opt => opt.MapFrom((src) => src.Tier))
				.ForMember(d => d.Ranking, opt => opt.MapFrom((src) => src.Ranking))
				.ForMember(d => d.HexColor, opt => opt.MapFrom((src) => TeamFightTacticsRank.RankedColorMap[!string.IsNullOrEmpty(src.Tier) ? src.Tier.ToLower() : "unranked"]));

			});
			serviceCollection.AddTransient<RemoteLeagueClient>();
			serviceCollection.AddSingleton<LocalLeagueClient>();
			serviceCollection.AddSingleton<RiotFileSystemService>();
			serviceCollection.AddSingleton<LeagueFileSystemService>();
			serviceCollection.AddSingleton<ILeagueClient, RemoteLeagueClient>();
			serviceCollection.AddSingleton<IRiotClient, RiotClient>();
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
				.AddImplementation<TeamFightTacticsPlatformService>(AccountType.TFT)
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
