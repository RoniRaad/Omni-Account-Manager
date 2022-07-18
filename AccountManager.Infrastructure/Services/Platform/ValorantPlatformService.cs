﻿using AccountManager.Core.Enums;
using AccountManager.Core.Exceptions;
using AccountManager.Core.Factories;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Models.RiotGames.Requests;
using AccountManager.Core.Models.RiotGames.Valorant;
using AccountManager.Core.Models.RiotGames.Valorant.Responses;
using AccountManager.Core.Services;
using AccountManager.Infrastructure.CachedClients;
using AccountManager.Infrastructure.Clients;
using AccountManager.Infrastructure.Services.FileSystem;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Reflection;

namespace AccountManager.Infrastructure.Services.Platform
{
    public class ValorantPlatformService : IPlatformService
    {
        private readonly ITokenService _riotService;
        private readonly IValorantClient _valorantClient;
        private readonly IRiotClient _riotClient;
        private readonly RiotFileSystemService _riotFileSystemService;
        private readonly AlertService _alertService;
        private readonly IMemoryCache _memoryCache;
        private readonly HttpClient _httpClient;
        private readonly IMapper _mapper;
        private readonly IUserSettingsService<UserSettings> _settingsService;
        public static string WebIconFilePath = Path.Combine("logos", "valorant-logo.svg");
        public static string IcoFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location)
            ?? ".", "ShortcutIcons", "valorant-logo.ico");
        public ValorantPlatformService(IRiotClient riotClient, GenericFactory<AccountType, ITokenService> tokenServiceFactory,
            IHttpClientFactory httpClientFactory, RiotFileSystemService riotLockFileService, AlertService alertService, 
            IMemoryCache memoryCache, IMapper mapper, IUserSettingsService<UserSettings> settingsService, IValorantClient valorantClient)
        {
            _riotClient = riotClient;
            _riotService = tokenServiceFactory.CreateImplementation(AccountType.Valorant);
            _httpClient = httpClientFactory.CreateClient("SSLBypass");
            _riotFileSystemService = riotLockFileService;
            _alertService = alertService;
            _memoryCache = memoryCache;
            _mapper = mapper;
            _settingsService = settingsService;
            _valorantClient = valorantClient;
        }
        private async Task<bool> TryLoginUsingRCU(Account account)
        {
            try
            {
                foreach (var process in Process.GetProcesses())
                    if (process.ProcessName.Contains("League") || process.ProcessName.Contains("Riot"))
                        process.Kill();

                await _riotFileSystemService.WaitForClientClose();
                _riotFileSystemService.DeleteLockfile();

                var startRiot = new ProcessStartInfo
                {
                    FileName = GetRiotExePath(),
                };
                Process.Start(startRiot);

                await _riotFileSystemService.WaitForClientInit();

                if (!_riotService.TryGetPortAndToken(out var token, out var port))
                    return false;

                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"riot:{token}")));
                await _httpClient.DeleteAsync($"https://127.0.0.1:{port}/player-session-lifecycle/v1/session");

                var lifeCycleResponse = await _httpClient.PostAsJsonAsync($"https://127.0.0.1:{port}/player-session-lifecycle/v1/session", new RiotClientApi.AuthFlowStartRequest
                {
                    LoginStrategy = "riot_identity",
                    PersistLogin = true,
                    RequireRiotID = true,
                    Scopes = new()
                    {
                        "openid",
                        "offline_access",
                        "lol",
                        "ban",
                        "profile",
                        "email",
                        "phone",
                        "account"
                    }
                });

                var resp = await _httpClient.PutAsJsonAsync($"https://127.0.0.1:{port}/rso-auth/v1/session/credentials", new RiotClientApi.LoginRequest
                {
                    Username = account.Username,
                    Password = account.Password,
                    PersistLogin = true,
                    Region = "NA1"
                });

                var credentialsResponse = await resp.Content.ReadFromJsonAsync<RiotClientApi.CredentialsResponse>();

                if (string.IsNullOrEmpty(credentialsResponse?.Type))
                {
                    _alertService.AddErrorMessage("There was an error signing in, please try again later.");
                }

                if (string.IsNullOrEmpty(credentialsResponse?.Multifactor?.Email))
                {
                    StartValorant();
                    return true;
                }

                var mfaCode = await _alertService.PromptUserFor2FA(account, credentialsResponse.Multifactor.Email);

                await _httpClient.PutAsJsonAsync($"https://127.0.0.1:{port}/rso-auth/v1/session/multifactor", new RiotClientApi.MultifactorLoginResponse
                {
                    Code = mfaCode,
                    Retry = false,
                    TrustDevice = true
                });

                StartValorant();
                return true;
            }
            catch (RiotClientNotFoundException)
            {
                _alertService.AddErrorMessage("Could not find riot client. Please set your riot install location in the settings page.");
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> TryLoginUsingApi(Account account)
        {
            try
            {
                foreach (var process in Process.GetProcesses())
                    if (process.ProcessName.Contains("League") || process.ProcessName.Contains("Riot"))
                        process.Kill();

                await _riotFileSystemService.WaitForClientClose();
                _riotFileSystemService.DeleteLockfile();

                var request = new RiotSessionRequest
                {
                    Id = "riot-client",
                    Nonce = "1",
                    RedirectUri = "http://localhost/redirect",
                    ResponseType = "token id_token",
                    Scope = "openid offline_access lol ban profile email phone account"
                };

                var authResponse = await _riotClient.RiotAuthenticate(request, account);
                if (authResponse is null || authResponse?.Cookies?.Tdid is null || authResponse?.Cookies?.Ssid is null ||
                    authResponse?.Cookies?.Sub is null || authResponse?.Cookies?.Csid is null)
                {
                    _alertService.AddErrorMessage("There was an issue authenticating with riot. We are unable to sign you in.");
                    return true;
                }

                await _riotFileSystemService.WriteRiotYaml("NA", authResponse.Cookies.Tdid.Value, authResponse.Cookies.Ssid.Value,
                    authResponse.Cookies.Sub.Value, authResponse.Cookies.Csid.Value);

                StartValorant();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task Login(Account account)
        {
            if (await TryLoginUsingApi(account))
                return;
            if (await TryLoginUsingRCU(account))
                return;

            _alertService.AddErrorMessage("There was an error attempting to sign in.");
        }

        private void StartValorant()
        {
            var startValorantCommandline = "--launch-product=valorant --launch-patchline=live";
            var startValorant = new ProcessStartInfo
            {
                FileName = GetRiotExePath(),
                Arguments = startValorantCommandline
            };
            Process.Start(startValorant);
        }

        public async Task<(bool, Rank)> TryFetchRank(Account account)
        {
            var rankCacheString = $"{account.Username}.valorant.rank";
            if (_memoryCache.TryGetValue(rankCacheString, out Rank? rank) && rank is not null)
                return (true, rank);

            rank = new Rank();
            try
            {
                if (string.IsNullOrEmpty(account.PlatformId))
                    account.PlatformId = await _riotClient.GetPuuId(account);
                if (string.IsNullOrEmpty(account.PlatformId))
                    return (false, rank);

                rank = await _valorantClient.GetValorantRank(account);

                if (!string.IsNullOrEmpty(rank?.Tier))
                    _memoryCache.Set(rankCacheString, rank, TimeSpan.FromHours(1));

                if (rank is null)
                    return (false, new Rank());

                return new(true, rank);
            }
            catch
            {
                return new(false, new Rank());
            }
        }

        public async Task<(bool, string)> TryFetchId(Account account)
        {
            try
            {
                if (!string.IsNullOrEmpty(account.PlatformId))
                {
                    return new (true, account.PlatformId);
                }

                var id = await _riotClient.GetPuuId(account);
                return new(id is not null, id ?? string.Empty);
            }
            catch
            {
                return new (false, string.Empty);
            }
        }

        private string GetRiotExePath()
        {
            var exePath = @$"{_settingsService.Settings.RiotInstallDirectory}\Riot Client\RiotClientServices.exe";
            if (!File.Exists(exePath))
                throw new RiotClientNotFoundException();

            return exePath;
        }
    }
}
