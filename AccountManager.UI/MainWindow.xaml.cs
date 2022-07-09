using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Windows;
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
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
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
				.ForMember(d => d.Ranking, opt => opt.MapFrom((src) => src != 0 ? new string('I', src % 3 + 1) : ""))
				.ForMember(d => d.HexColor, opt => opt.MapFrom((src) => ValorantRank.RankedColorMap[ValorantRank.RankMap[src / 3].ToLower()]));

				cfg.CreateMap<Rank, LeagueRank>()
				.ForMember(d => d.Tier, opt => opt.MapFrom((src) => src.Tier))
				.ForMember(d => d.Ranking, opt => opt.MapFrom((src) => src.Ranking))
				.ForMember(d => d.HexColor, opt => opt.MapFrom((src) => LeagueRank.RankedColorMap[!string.IsNullOrEmpty(src.Tier) ? src.Tier.ToLower() : "unranked"]));

				cfg.CreateMap<Rank, TeamFightTacticsRank>()
				.ForMember(d => d.Tier, opt => opt.MapFrom((src) => src.Tier))
				.ForMember(d => d.Ranking, opt => opt.MapFrom((src) => src.Ranking))
				.ForMember(d => d.HexColor, opt => opt.MapFrom((src) => TeamFightTacticsRank.RankedColorMap[!string.IsNullOrEmpty(src.Tier) ? src.Tier.ToLower() : "unranked"]));

				cfg.CreateMap<string, ValorantCharacter>()
				.ForMember(d => d.Name, opt => opt.MapFrom((src) => ValorantCharacter.CharacterMapping.ContainsKey(src) ? ValorantCharacter.CharacterMapping[src] : "UNKNOWN CHARACTER"))
				.ReverseMap();

				cfg.CreateMap<MatchHistory, MatchHistoryResponse>()
				.ReverseMap();
				cfg.CreateMap<MatchHistory.Json, MatchHistoryResponse.Json>()
				.ReverseMap();
				cfg.CreateMap<MatchHistory.Metadata, MatchHistoryResponse.Metadata>()
				.ReverseMap();
				cfg.CreateMap<MatchHistory.Participant, MatchHistoryResponse.Participant>()
				.ReverseMap();
				cfg.CreateMap<MatchHistory.Ban, MatchHistoryResponse.Ban>()
				.ReverseMap();
				cfg.CreateMap<MatchHistory.Team, MatchHistoryResponse.Team>()
				.ReverseMap();
				cfg.CreateMap<MatchHistory.Participant, MatchHistoryResponse.Participant>()
				.ReverseMap();
				cfg.CreateMap<MatchHistory.Objectives, MatchHistoryResponse.Objectives>()
				.ReverseMap();
				cfg.CreateMap<MatchHistory.Baron, MatchHistoryResponse.Baron>()
				.ReverseMap();
				cfg.CreateMap<MatchHistory.Challenges, MatchHistoryResponse.Challenges>()
				.ReverseMap();
				cfg.CreateMap<MatchHistory.Champion, MatchHistoryResponse.Champion>()
				.ReverseMap();
				cfg.CreateMap<MatchHistory.Dragon, MatchHistoryResponse.Dragon>()
				.ReverseMap();
				cfg.CreateMap<MatchHistory.Game, MatchHistoryResponse.Game>()
				.ReverseMap();
				cfg.CreateMap<MatchHistory.Inhibitor, MatchHistoryResponse.Inhibitor>()
				.ReverseMap();
				cfg.CreateMap<MatchHistory.Json, MatchHistoryResponse.Json>()
				.ReverseMap();
				cfg.CreateMap<MatchHistory.Metadata, MatchHistoryResponse.Metadata>()
				.ReverseMap();
				cfg.CreateMap<MatchHistory.Participant, MatchHistoryResponse.Participant>()
				.ReverseMap();
				cfg.CreateMap<MatchHistory.Perks, MatchHistoryResponse.Perks>()
				.ReverseMap();
				cfg.CreateMap<MatchHistory.RiftHerald, MatchHistoryResponse.RiftHerald>()
				.ReverseMap();
				cfg.CreateMap<MatchHistory.Selection, MatchHistoryResponse.Selection>()
				.ReverseMap();
				cfg.CreateMap<MatchHistory.StatPerks, MatchHistoryResponse.StatPerks>()
				.ReverseMap();
				cfg.CreateMap<MatchHistory.Style, MatchHistoryResponse.Style>()
				.ReverseMap();
				cfg.CreateMap<MatchHistory.Tower, MatchHistoryResponse.Tower>()
				.ReverseMap();

				cfg.AllowNullDestinationValues = true;
			});
			serviceCollection.AddTransient<LeagueClient>();
			serviceCollection.AddSingleton<LeagueTokenClient>();
			serviceCollection.AddSingleton<RiotFileSystemService>();
			serviceCollection.AddSingleton<ValorantClient>();
            serviceCollection.AddSingleton<LeagueFileSystemService>();
            serviceCollection.AddSingleton<ValorantGraphService>();
            serviceCollection.AddSingleton<ILeagueClient, LeagueClient>();
			serviceCollection.AddSingleton<IValorantClient>((services) => new CachedValorantClient(services.GetRequiredService<IMemoryCache>(), services.GetRequiredService<IDistributedCache>(), services.GetRequiredService<ValorantClient>()));
			serviceCollection.AddSingleton<RiotClient>();
			serviceCollection.AddSingleton<LeagueClient>();
			serviceCollection.AddSingleton<IRiotClient>((services) => new CachedRiotClient(services.GetRequiredService<IMemoryCache>(), services.GetRequiredService<RiotClient>()));
			serviceCollection.AddSingleton<ILeagueClient>((services) => new CachedLeagueClient(services.GetRequiredService<IMemoryCache>(), services.GetRequiredService<LeagueClient>()));
			serviceCollection.AddSingleton<ICurlRequestBuilder, CurlRequestBuilder>();
			serviceCollection.AddSingleton<LeagueGraphService>();
			serviceCollection.AddSingleton<ILeagueGraphService>((services) => new CachedLeagueGraphService(services.GetRequiredService<IDistributedCache>(), services.GetRequiredService<LeagueGraphService>()));
			serviceCollection.AddSingleton<IValorantGraphService>((services) => new CachedValorantGraphService(services.GetRequiredService<IDistributedCache>(), services.GetRequiredService<ValorantGraphService>()));
			serviceCollection.AddSingleton<ITeamFightTacticsGraphService, TeamFightTacticsGraphService>();
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
