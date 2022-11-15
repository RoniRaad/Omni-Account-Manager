using AccountManager.Core.Enums;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models.AppSettings;
using AccountManager.Core.Models.UserSettings;
using AccountManager.Core.Services.GraphServices.Cached;
using AccountManager.Core.Services.GraphServices;
using AccountManager.Core.Services;
using AccountManager.Infrastructure.CachedClients;
using AccountManager.Infrastructure.Clients;
using AccountManager.Infrastructure.Services.FileSystem;
using AccountManager.Infrastructure.Services.Platform;
using AccountManager.Infrastructure.Services.Token;
using AccountManager.Infrastructure.Services;
using AccountManager.UI.Extensions;
using AccountManager.UI.Services;
using Blazorise;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NeoSmart.Caching.Sqlite;
using Plk.Blazor.DragDrop;
using System.Collections.Generic;
using System;
using Microsoft.Extensions.Configuration;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;

namespace AccountManager.UI
{
    public static class Startup
    {
        public static void ConfigureServices(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddBlazorWebView();
            services.AddBlazorDragDrop();
            services.AddOptions();
            services.AddLogging(builder =>
            {
                builder.AddConfiguration(configuration.GetSection("Logging"));
                builder.AddFile(o => o.RootPath = AppContext.BaseDirectory);
            });
            services.AddSqliteCache(options => {
                options.CachePath = @$"{GeneralFileSystemService.DataPath}\cache.db";
            });
            services.AddMemoryCache();
            services.AddAutoMapperMappings();
            services.AddNamedClients(configuration);
            services.Configure<RiotApiUri>(configuration.GetSection("RiotApiUri"));
            services.Configure<AboutEndpoints>(configuration.GetSection("AboutEndpoints"));
            services.Configure<EpicGamesApiUri>(configuration.GetSection("EpicGamesApiUri"));
            services.AddSingleton<IAlertService, AlertService>();
            services.AddSingleton<IAccountFilterService, AccountFilterService>();
            services.AddState();
            services.AddAuth();
            services.AddLogging();
            services.AddSingleton<IRiotFileSystemService, RiotFileSystemService>();
            services.AddSingleton<LeagueFileSystemService>();
            services.AddSingleton<ISteamLibraryService, SteamLibraryService>();
            services.AddSingleton<IShortcutService, ShortcutService>();
            services.AddSingleton<IAppUpdateService, SquirrelAppUpdateService>();
            services.AddSingleton<IEpicGamesExternalAuthService, EpicGamesExternalAuthService>();
            services.AddTransient<IEpicGamesTokenClient, EpicGamesTokenClient>();
            services.AddTransient<IEpicGamesLibraryService, EpicGamesLibraryService>();
            services.AddSingleton<IGeneralFileSystemService, CachedGeneralFileSystemService>();
            services.AddSingleton<IAccountExportService, AccountExportService>();
            services.AddSingleton<IRiotThirdPartyClient, CachedRiotThirdPartyClient>();

            // Cached Objects
            services.AddSingleton<RiotThirdPartyClient>();
            services.AddSingleton<RiotClient>();
            services.AddSingleton<LeagueClient>();
            services.AddSingleton<ValorantClient>();
            services.AddTransient<LeagueClient>();
            services.AddTransient<LeagueTokenClient>();
            services.AddSingleton<ValorantGraphService>();
            services.AddSingleton<LeagueGraphService>();
            services.AddSingleton<RiotTokenClient>();
            services.AddSingleton<GeneralFileSystemService>(); 

            services.AddSingleton<IRiotTokenClient>((services) => new CachedRiotTokenClient(services.GetRequiredService<IMemoryCache>(), services.GetRequiredService<RiotTokenClient>()));
            services.AddSingleton<ILeagueTokenClient>((services) => new CachedLeagueTokenClient(services.GetRequiredService<IMemoryCache>(), services.GetRequiredService<LeagueTokenClient>()));
            services.AddSingleton<IValorantClient>((services) => new CachedValorantClient(services.GetRequiredService<IMemoryCache>(), services.GetRequiredService<IDistributedCache>(), services.GetRequiredService<ValorantClient>()));
            services.AddSingleton<IRiotClient>((services) => new CachedRiotClient(services.GetRequiredService<IMemoryCache>(), services.GetRequiredService<RiotClient>()));
            services.AddSingleton<ILeagueClient>((services) => new CachedLeagueClient(services.GetRequiredService<IMemoryCache>(), services.GetRequiredService<LeagueClient>()));
            services.AddSingleton<ILeagueGraphService>((services) => new CachedLeagueGraphService(services.GetRequiredService<IDistributedCache>(), services.GetRequiredService<LeagueGraphService>()));
            services.AddSingleton<IValorantGraphService>((services) => new CachedValorantGraphService(services.GetRequiredService<IDistributedCache>(), services.GetRequiredService<ValorantGraphService>()));
            services.AddSingleton<IHttpRequestBuilder, CurlRequestBuilder>();
            services.AddSingleton<ITeamFightTacticsGraphService, TeamFightTacticsGraphService>();
            services.AddSingleton<IIpcService, IpcService>();
            services.AddSingleton<IHttpRequestBuilder, CurlRequestBuilder>();
            services.AddSingleton<IHttpRequestBuilder, CurlRequestBuilder>();
            services.AddBlazorise(options =>
            {
                options.Immediate = true;
            })
            .AddBootstrapProviders()
            .AddFontAwesomeIcons();
            services.AddSingleton<IAccountService, AccountService>();
            services.AddSingleton<IUserSettingsService<GeneralSettings>, UserSettingsService<GeneralSettings>>();
            services.AddSingleton<IUserSettingsService<SteamSettings>, UserSettingsService<SteamSettings>>();
            services.AddSingleton<IUserSettingsService<LeagueSettings>, UserSettingsService<LeagueSettings>>();
            services.AddSingleton<IUserSettingsService<Dictionary<Guid, AccountListItemSettings>>, UserSettingsService<Dictionary<Guid, AccountListItemSettings>>>();
            services.AddFactory<AccountType, IPlatformService>()
                    .AddImplementation<SteamPlatformService>(AccountType.Steam)
                    .AddImplementation<LeaguePlatformService>(AccountType.League)
                    .AddImplementation<TeamFightTacticsPlatformService>(AccountType.TeamFightTactics)
                    .AddImplementation<ValorantPlatformService>(AccountType.Valorant)
                    .AddImplementation<EpicGamesPlatformService>(AccountType.EpicGames)
                    .Build();
            services.AddFactory<AccountType, ITokenService>()
                    .AddImplementation<LeagueTokenService>(AccountType.League)
                    .AddImplementation<LeagueTokenService>(AccountType.TeamFightTactics)
                    .AddImplementation<RiotTokenService>(AccountType.Valorant)
                    .Build();
        }
    }
}
