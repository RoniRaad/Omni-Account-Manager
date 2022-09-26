using AccountManager.Core.Enums;
using AccountManager.Core.Exceptions;
using AccountManager.Core.Factories;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Models.RiotGames;
using AccountManager.Core.Models.RiotGames.Requests;
using AccountManager.Core.Models.UserSettings;
using AccountManager.Core.Services;
using AccountManager.Infrastructure.Clients;
using AccountManager.Infrastructure.Services.FileSystem;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Reflection;

namespace AccountManager.Infrastructure.Services.Platform
{
    public sealed class ValorantPlatformService : IPlatformService
    {
        private readonly ITokenService _riotService;
        private readonly IValorantClient _valorantClient;
        private readonly IRiotClient _riotClient;
        private readonly IRiotFileSystemService _riotFileSystemService;
        private readonly IAlertService _alertService;
        private readonly ILogger<ValorantPlatformService> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly HttpClient _httpClient;
        private readonly IRiotTokenClient _riotTokenClient;
        private readonly IUserSettingsService<GeneralSettings> _settingsService;
        public static readonly string WebIconFilePath = Path.Combine("logos", "valorant-logo.svg");
        public static readonly string IcoFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location)
            ?? ".", "ShortcutIcons", "valorant-logo.ico");
        public ValorantPlatformService(IRiotClient riotClient, IGenericFactory<AccountType, ITokenService> tokenServiceFactory,
            IHttpClientFactory httpClientFactory, IRiotFileSystemService riotLockFileService, IAlertService alertService,
            IMemoryCache memoryCache, IUserSettingsService<GeneralSettings> settingsService, IValorantClient valorantClient, 
            IRiotTokenClient riotTokenClient, ILogger<ValorantPlatformService> logger)
        {
            _riotClient = riotClient;
            _riotService = tokenServiceFactory.CreateImplementation(AccountType.Valorant);
            _httpClient = httpClientFactory.CreateClient("SSLBypass");
            _riotFileSystemService = riotLockFileService;
            _alertService = alertService;
            _memoryCache = memoryCache;
            _settingsService = settingsService;
            _valorantClient = valorantClient;
            _riotTokenClient = riotTokenClient;
            _logger = logger;
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

                await _httpClient.PostAsJsonAsync($"https://127.0.0.1:{port}/player-session-lifecycle/v1/session", new RiotClientApi.AuthFlowStartRequest
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
                    _alertService.AddErrorAlert("There was an error signing in, please try again later.");
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
                _alertService.AddErrorAlert("Could not find riot client. Please set your riot install location in the settings page.");
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> TryLoginUsingApi(Account account)
        {
            RegionInfo? regionInfo;
            try
            {
                _logger.LogInformation("Attempting to login to account! Username: {Username}, Account Type: {AccountType}", account.Username, account.AccountType);
                foreach (var process in Process.GetProcesses())
                    if (process.ProcessName.Contains("League") || process.ProcessName.Contains("Riot"))
                        process.Kill();

                await _riotFileSystemService.WaitForClientClose();
                _riotFileSystemService.DeleteLockfile();
                _logger.LogInformation("Riot client closed and lockfile deleted!");

                var request = new RiotTokenRequest
                {
                    Id = "riot-client",
                    Nonce = "1",
                    RedirectUri = "http://localhost/redirect",
                    ResponseType = "token id_token",
                    Scope = "openid offline_access lol ban profile email phone account"
                };

                _logger.LogInformation("Attempted to retrieve riot tokens!");

                var authResponse = await _riotTokenClient.GetRiotTokens(request, account);
                if (authResponse is null || authResponse?.Cookies?.Tdid is null || authResponse?.Cookies?.Ssid is null ||
                    authResponse?.Cookies?.Sub is null || authResponse?.Cookies?.Csid is null)
                {
                    _alertService.AddErrorAlert("There was an issue authenticating with riot. We are unable to sign you in.");
                    _logger.LogError("Unable to retrieve riot login cookies! There must have been an issue with the auth request! Account Username: {Username}", account.Username);
                    return true;
                }

                _logger.LogInformation("Riot token obtained successfully!");

                try
                {
                    regionInfo = await _riotClient.GetRegionInfo(account);
                }
                catch
                {
                    _logger.LogError("Unable to obtain region info for riot account! Account Username: {Username}", account.Username);
                    regionInfo = new();
                }

                await _riotFileSystemService.WriteRiotYaml(regionInfo.RegionId, authResponse.Cookies.Tdid.Value, authResponse.Cookies.Ssid.Value,
                    authResponse.Cookies.Sub.Value, authResponse.Cookies.Csid.Value);

                StartValorant();
                return true;
            }
            catch (RiotClientNotFoundException)
            {
                _alertService.AddErrorAlert("Could not find riot client. Please set your riot install location in the settings page.");
                _logger.LogError("Could not find riot client! Unable to login!");

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

            _alertService.AddErrorAlert("There was an error attempting to sign in.");
            _logger.LogError("Riot login failed via api and rcu!");
        }

        private void StartValorant()
        {
            _logger.LogInformation("Launching valorant...");

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
