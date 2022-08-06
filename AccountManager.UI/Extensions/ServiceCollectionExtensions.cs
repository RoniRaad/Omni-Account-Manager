using AccountManager.Core.Interfaces;
using AccountManager.Core.Models.RiotGames.League.Requests;
using AccountManager.Core.Models.RiotGames.League;
using AccountManager.Core.Models.RiotGames.Valorant;
using AccountManager.Core.Models;
using AccountManager.Core.Services;
using AccountManager.Infrastructure.Services;
using AccountManager.UI.Builders;
using IPC.NamedPipe;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using static AccountManager.Core.Services.AppState;
using AccountManager.Core.Models.AppSettings;
using System.Net.Http;
using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Distributed;
using AccountManager.Core.Static;
using System.Linq;

namespace AccountManager.UI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IGenericFactoryBuilder<TKey, TInterface> AddFactory<TKey, TInterface>(this ServiceCollection services) where TKey : notnull, new()
        {
            return new GenericFactoryBuilder<TKey, TInterface>(services);
        }

        public static IServiceCollection AddAuth(this ServiceCollection services)
        {
            var args = Environment.GetCommandLineArgs();

            if (args.Length > 1 & Array.Exists(args, element => element == "/login"))
            {
                var parsedArgs = ParseCommandLineArgs(args);

                services.AddSingleton((services) =>
                {
                    var persistantCache = services.GetRequiredService<IDistributedCache>();
                    var authService = new AuthService(services.GetRequiredService<IIOService>(), services.GetRequiredService<AlertService>(), persistantCache);

                    Task.Run(async () =>
                    {
                        if (await persistantCache.GetAsync<bool>("rememberPassword"))
                        {
                            var password = await persistantCache.GetAsync<string>("masterPassword");
                            if (!string.IsNullOrEmpty(password))
                            {
                                authService.Login(password);
                                var accountService = services.GetRequiredService<IAccountService>();
                                var accounts = accountService.GetAllAccountsMin();
                                await accountService.Login(accounts.FirstOrDefault((acc) => acc?.Guid.ToString() == parsedArgs["login"]) ?? new());
                                Environment.Exit(0);
                            }
                        }
                    });

                    return authService;
                });
            }
            else
                services.AddSingleton<AuthService>();

            return services;
        }

        public static IServiceCollection AddState(this ServiceCollection services)
        {
            string LoginCommand = "login";
            var args = Environment.GetCommandLineArgs();

            if (args.Length > 1)
            {
                var parsedArgs = ParseCommandLineArgs(args);
         
                if (parsedArgs.ContainsKey(LoginCommand))
                {
                    var processes = Process.GetProcessesByName("OmniAccountManager");
                    if (processes.Length > 1)
                    {
                        Node node = new("omni-account-manager", "omni-account-manager", "localhost", (arg) => { });
                        node.Start();
                        var argument = JsonSerializer.Serialize(new IpcLoginParameter() { Guid = new Guid(parsedArgs[LoginCommand]) });
                        node.Send($"IpcLogin:{argument}");
                        Environment.Exit(0);
                    }
                    else
                    {
                        services.AddSingleton<IAppState, AppState>((services) =>
                        {
                            var appState = new AppState(services.GetRequiredService<IAccountService>(), services.GetRequiredService<IIpcService>());
                            Task.Run(async () =>
                            {
                                await appState.IpcLogin(new IpcLoginParameter() { Guid = new Guid(parsedArgs[LoginCommand]) });
                                Environment.Exit(0);
                            });

                            return appState;
                        });
                    }
                }
            }
            else
            {
                services.AddSingleton<IAppState, AppState>();
            }

            return services;
        }

        public static IServiceCollection AddAutoMapperMappings(this ServiceCollection services)
        {
            services.AddAutoMapper((cfg) =>
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

            return services;
        }

        public static IServiceCollection AddNamedClients(this ServiceCollection services, IConfiguration configuration)
        {
            var riotApiUri = configuration.GetSection("RiotApiUri").Get<RiotApiUri>();

            services.AddHttpClient("RiotAuth", (httpClient) =>
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
            services.AddHttpClient("RiotEntitlement", (httpClient) =>
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
            services.AddHttpClient("RiotSessionNA", (httpClient) =>
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
            services.AddHttpClient("LeagueNA", (httpClient) =>
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
            services.AddHttpClient("Valorant", (httpClient) =>
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
            services.AddHttpClient("ValorantNA", (httpClient) =>
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
            services.AddHttpClient("SSLBypass").ConfigureHttpMessageHandlerBuilder(x =>
            {
                var httpClientHandler = new HttpClientHandler();
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
                {
                    return true;
                };

                x.PrimaryHandler = httpClientHandler;
            });

            return services;
        }

        private static Dictionary<string, string> ParseCommandLineArgs(string[] args)
        {
            args = args[1..];
            var parsedArgs = new Dictionary<string, string>();
            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                if (arg.StartsWith("/"))
                {
                    parsedArgs[arg[1..]] = "";

                    if (args.Length != i - 1)
                    {
                        parsedArgs[arg[1..]] = args[i + 1];
                    }
                }
            }

            return parsedArgs;
        }
    }
}
