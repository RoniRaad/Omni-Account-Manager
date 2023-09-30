using AccountManager.Core.Enums;
using AccountManager.Core.Exceptions;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Models.EpicGames;
using AccountManager.Core.Models.RiotGames;
using AccountManager.Core.Models.RiotGames.Requests;
using AccountManager.Core.Models.UserSettings;
using AccountManager.Core.Static;
using AccountManager.Infrastructure.Clients;
using AccountManager.Infrastructure.Services.FileSystem;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;

namespace AccountManager.Infrastructure.Services.Platform
{
    public sealed class RiotPlatformService : IPlatformService
    {
        private readonly ITokenService _riotService;
        private readonly IRiotClient _riotClient;
        private readonly IRiotFileSystemService _riotFileSystemService;
        private readonly IAlertService _alertService;
        private readonly ILogger<RiotPlatformService> _logger;
        private readonly IDistributedCache _persistantCache;
        private readonly HttpClient _httpClient;
        private readonly IRiotTokenClient _riotTokenClient;
        private readonly IUserSettingsService<GeneralSettings> _settingsService;
        public static readonly string WebIconFilePath = Path.Combine("logos", "valorant-logo.svg");
        public static readonly string IcoFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location)
            ?? ".", "ShortcutIcons", "valorant-logo.ico");
        public RiotPlatformService(IRiotClient riotClient, IGenericFactory<AccountType, ITokenService> tokenServiceFactory,
            IHttpClientFactory httpClientFactory, IRiotFileSystemService riotLockFileService, IAlertService alertService,
            IDistributedCache persistantCache, IUserSettingsService<GeneralSettings> settingsService, 
            IRiotTokenClient riotTokenClient, ILogger<RiotPlatformService> logger)
        {
            _riotClient = riotClient;
            _riotService = tokenServiceFactory.CreateImplementation(AccountType.Valorant);
            _httpClient = httpClientFactory.CreateClient("SSLBypass");
            _riotFileSystemService = riotLockFileService;
            _alertService = alertService;
            _persistantCache = persistantCache;
            _settingsService = settingsService;
            _riotTokenClient = riotTokenClient;
            _logger = logger;
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

        public async Task<(bool, Rank)> TryFetchRank(Account account)
        {
            return new(true, new Rank() { Tier="N/A"});
        }

        public async Task<(bool, string)> TryFetchId(Account account)
        {
            try
            {
                if (!string.IsNullOrEmpty(account.PlatformId))
                {
                    return new(true, account.PlatformId);
                }

                var id = await _riotClient.GetPuuId(account);
                return new(id is not null, id ?? string.Empty);
            }
            catch
            {
                return new(false, string.Empty);
            }
        }

        private async Task<bool> TryLoginUsingRCU(Account account)
        {
            try
            {
                CloseAllRiotApps();

                await _riotFileSystemService.WaitForClientClose();
                _riotFileSystemService.DeleteLockfile();

                var startRiot = new ProcessStartInfo
                {
                    FileName = GetRiotExePath(),
                };

                Process.Start(startRiot);

                await _riotFileSystemService.WaitForClientInit();

                if (!await SendCredentialsToClient(account))
                {
                    return false;
                }

                StartRiot();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> TryLoginUsingApi(Account account)
        {
            RegionInfo regionInfo;
            try
            {
                _logger.LogInformation("Attempting to login to account! Username: {Username}, Account Type: {AccountType}", account.Username, account.AccountType);

                try
                {
                    regionInfo = await _riotClient.GetValorantRegionInfo(account);
                }
                catch
                {
                    _logger.LogError("Unable to obtain region info for riot account! Account Username: {Username}", account.Username);
                    regionInfo = new();
                }

                CloseAllRiotApps();

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

                var riotTokens = await _riotTokenClient.GetRiotTokens(request, account);
                if (riotTokens is null || riotTokens?.Cookies?.Tdid is null || riotTokens?.Cookies?.Ssid is null ||
                    riotTokens?.Cookies?.Sub is null || riotTokens?.Cookies?.Csid is null)
                {
                    _logger.LogError("Unable to retrieve riot login cookies! There must have been an issue with the auth request! Account Username: {Username}", account.Username);

                    return false;
                }

                _logger.LogInformation("Riot token obtained successfully!");

                JwtSecurityTokenHandler jwtSecurityTokenHandler = new();
                var jwtToken = jwtSecurityTokenHandler.ReadJwtToken(riotTokens.AccessToken);
                jwtToken.Payload.TryGetValue("dat", out object? regionCodeObject);
                var regionCodeTokenJson = regionCodeObject?.ToString();
                var region = "NA";

                if (regionCodeTokenJson is not null)
                {
                    var accessTokenPayloadData = JsonSerializer.Deserialize<RiotAccessTokenPayloadData>(regionCodeTokenJson);
                    region = accessTokenPayloadData?.Region?.ToUpper()?.TrimEnd('1') ?? "NA";
                }

                await _riotFileSystemService.WriteRiotYaml(region, riotTokens.Cookies.Tdid.Value, riotTokens.Cookies.Ssid.Value,
                    riotTokens.Cookies.Sub.Value, riotTokens.Cookies.Csid.Value);

                StartRiot();

                if (!await VerifyLogInStatus() && !await SendCredentialsToClient(account))
                {
                    return false;
                }

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

        private async Task<bool> SendCredentialsToClient(Account account)
        {
            if (!_riotService.TryGetPortAndToken(out var token, out var port))
                return false;

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"riot:{token}")));

            await _httpClient.DeleteAsync($"https://127.0.0.1:{port}/rso-auth/v1/session");

            await _httpClient.PostAsJsonAsync($"https://127.0.0.1:{port}/rso-auth/v2/authorizations", new RcuAuthorizationsRequest
            {
                ClientId = "riot-client",
                TrustLevels = new() { "always_trusted" }
            });

            var resp = await _httpClient.PutAsJsonAsync($"https://127.0.0.1:{port}/rso-auth/v1/session/credentials", new RiotClientApi.LoginRequest
            {
                Username = account.Username,
                Password = account.Password,
                PersistLogin = true,
            });

            var credentialsResponse = await resp.Content.ReadFromJsonAsync<RiotClientApi.CredentialsResponse>();

            if (string.IsNullOrEmpty(credentialsResponse?.Type))
            {
                _alertService.AddErrorAlert("There was an error signing in, please try again later.");
                return false;
            }

            if (string.IsNullOrEmpty(credentialsResponse?.Multifactor?.Email))
                return true;

            var mfaCode = await _alertService.PromptUserFor2FA(account, credentialsResponse.Multifactor.Email);

            await _httpClient.PutAsJsonAsync($"https://127.0.0.1:{port}/rso-auth/v1/session/multifactor", new RiotClientApi.MultifactorLoginResponse
            {
                Code = mfaCode,
                Retry = false,
                TrustDevice = true
            });

            return true;
        }

        private void CloseAllRiotApps()
        {
            foreach (var process in Process.GetProcesses())
                if (process.ProcessName.ToLower().Contains("league")
                    || process.ProcessName.ToLower().Contains("riot")
                    || process.ProcessName.ToLower().Contains("valorant"))
                    process.Kill();
        }

        private async Task<bool> VerifyLogInStatus()
        {
            await _riotFileSystemService.WaitForClientInit();

            if (!_riotService.TryGetPortAndToken(out var token, out var port))
                return false;

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"riot:{token}")));
            var authorizationStatus = await _httpClient.GetAsync($"https://127.0.0.1:{port}/rso-auth/v1/authorization");
            if (!authorizationStatus.IsSuccessStatusCode)
                return false;

            return true;
        }

        private void StartRiot()
        {
            _logger.LogInformation("Launching riot...");

            var startRiot = new ProcessStartInfo
            {
                FileName = GetRiotExePath()
            };
            Process.Start(startRiot);
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
