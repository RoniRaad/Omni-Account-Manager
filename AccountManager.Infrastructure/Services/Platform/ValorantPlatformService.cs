using AccountManager.Core.Enums;
using AccountManager.Core.Factories;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Models.RiotGames.League.Requests;
using AccountManager.Core.Services;
using AccountManager.Infrastructure.Services.FileSystem;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace AccountManager.Infrastructure.Services.Platform
{
    public class ValorantPlatformService : IPlatformService
    {
        private readonly ITokenService _riotService;
        private readonly IRiotClient _riotClient;
        private readonly RiotLockFileService _riotFileSystemService;
        private readonly AlertService _alertService;
        private readonly HttpClient _httpClient;
        private Dictionary<string, string> RankColorMap = new Dictionary<string, string>()
        {
            {"iron", "#3a3a3a"},
            {"bronze", "#823012"},
            {"silver", "#999c9b"},
            {"gold", "#e2cd5f"},
            {"platinum", "#308798"},
            {"diamond", "#f195f4"},
            {"immortal", "#ac3654"},
        };
        public ValorantPlatformService(IRiotClient riotClient, GenericFactory<AccountType, ITokenService> tokenServiceFactory, 
            IHttpClientFactory httpClientFactory, RiotLockFileService riotLockFileService, AlertService alertService)
        {
            _riotClient = riotClient;
            _riotService = tokenServiceFactory.CreateImplementation(AccountType.Valorant);
            _httpClient = httpClientFactory.CreateClient("SSLBypass");
            _riotFileSystemService = riotLockFileService;
            _alertService = alertService;
        }
        public async Task Login(Account account)
        {
            string token;
            string port;
            EventHandler riotClientOpen = null;
            try
            {
                foreach (var process in Process.GetProcesses())
                    if (process.ProcessName.Contains("League") || process.ProcessName.Contains("Riot"))
                        process.Kill();

                Process.Start(GetRiotExePath());

                await _riotFileSystemService.WaitForClientInit();

                var signInRequest = new LeagueSignInRequest
                {
                    Username = account.Username,
                    Password = account.Password,
                    StaySignedIn = true
                };

                _riotService.TryGetPortAndToken(out token, out port);

                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"riot:{token}")));
                _ = await _httpClient.DeleteAsync($"https://127.0.0.1:{port}/rso-auth/v1/session");
                var sessionCreateResponse = await _httpClient.PostAsJsonAsync($"https://127.0.0.1:{port}/rso-auth/v2/authorizations", new CreateAuthorizations());
                var Sesestr = await sessionCreateResponse.Content.ReadAsStringAsync();

                var loginResponse = await _httpClient.PutAsJsonAsync($"https://127.0.0.1:{port}/rso-auth/v1/session/credentials", signInRequest);
                var loginResponseStr = await loginResponse.Content.ReadAsStringAsync();
                var loginResponseObj = await loginResponse.Content.ReadFromJsonAsync<RiotLoginResponse>();

                if (!string.IsNullOrEmpty(loginResponseObj.Error))
                {
                    if (loginResponseObj.Error == "rate_limited")
                    {
                        _alertService.ErrorMessage = "Error logging in, too many attempts made. Try again later.";
                        return;
                    }
                }

                if (!string.IsNullOrEmpty(loginResponseObj?.Multifactor?.Email))
                {
                    var twoFactorCode = await _alertService.PromptUserFor2FA(account, loginResponseObj?.Multifactor?.Email);
                    var mfLogin = await _httpClient.PutAsJsonAsync($"https://127.0.0.1:{port}/rso-auth/v1/session/multifactor", new MultifactorRequest()
                    {
                        Code = twoFactorCode,
                        Retry = false,
                        TrustDevice = false
                    });
                    var mfLoginResponse = await mfLogin.Content.ReadFromJsonAsync<RiotLoginResponse>();

                    if (!string.IsNullOrEmpty(mfLoginResponse?.Multifactor?.Email))
                    {
                        _alertService.ErrorMessage = "Incorrect code. Login failed.";
                        return;
                    }

                    StartValorant();
                }
                else
                {
                    StartValorant();
                }
            }
            catch
            {
                _alertService.ErrorMessage = "There was an error signing in.";
            }
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
            Rank rank = new Rank();

            try
            {
                account.PlatformId ??= await _riotClient.GetPuuId(account.Username, account.Password);

                rank = await _riotClient.GetValorantRank(account);
                SetRankColor(rank);
                return new(true, rank);
            }
            catch
            {
                return new(false, rank);
            }
        }

        public async Task<(bool, string)> TryFetchId(Account account)
        {
            string id = "";

            try
            {
                if (!string.IsNullOrEmpty(account.PlatformId))
                {
                    return new (true, account.PlatformId);
                }

                id = await _riotClient.GetPuuId(account.Username, account.Password);
                return new(true, id);
            }
            catch
            {
                return new (false, id);
            }
        }
        private void SetRankColor(Rank rank)
        {
            foreach (KeyValuePair<string, string> kvp in RankColorMap)
                if (rank.Tier.ToLower().Equals(kvp.Key))
                    rank.Color = kvp.Value;
        }
        private DriveInfo FindRiotDrive()
        {
            DriveInfo riotDrive = null;
            foreach (DriveInfo drive in DriveInfo.GetDrives())
                if (Directory.Exists($"{drive.RootDirectory}\\Riot Games"))
                    riotDrive = drive;

            return riotDrive;
        }
        private string GetRiotExePath()
        {
            return @$"{FindRiotDrive().RootDirectory}\Riot Games\Riot Client\RiotClientServices.exe";
        }
    }
    public class Multifactor
    {
        [JsonPropertyName("email")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Email { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [JsonPropertyName("method")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Method { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [JsonPropertyName("methods")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public List<object> Methods { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [JsonPropertyName("mfaVersion")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public object MfaVersion { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [JsonPropertyName("multiFactorCodeLength")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public object MultiFactorCodeLength { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }

    public class RiotLoginResponse
    {
        [JsonPropertyName("authenticationType")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string AuthenticationType { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [JsonPropertyName("country")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Country { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [JsonPropertyName("error")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Error { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [JsonPropertyName("multifactor")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Multifactor Multifactor { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [JsonPropertyName("persistLogin")]
        public bool PersistLogin { get; set; }

        [JsonPropertyName("securityProfile")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public object SecurityProfile { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [JsonPropertyName("type")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Type { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }
    public class MultifactorRequest
    {
        [JsonPropertyName("code")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Code { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [JsonPropertyName("retry")]
        public bool Retry { get; set; }

        [JsonPropertyName("trustDevice")]
        public bool TrustDevice { get; set; }
    }

    public class CreateAuthorizations
    {
        [JsonPropertyName("authenticationType")]
        public string AuthenticationType { get; set; } = "SSOAuth";
        [JsonPropertyName("claims")]
        public List<string> Claims { get; set; } = new List<string>()
        {
            "sub","iss","auth_time","acr","name"
        };
        [JsonPropertyName("scope")]
        public List<string> Scope { get; set; } = new List<string>()
        {
            "openid","profile","email","lol","summoner"
        };
        [JsonPropertyName("clientId")]
        public string ClientId { get; set; } = "riot-client";

        [JsonPropertyName("keepAlive")]
        public bool KeepAlive { get; set; } = true;

        [JsonPropertyName("trustLevels")]
        public List<string> TrustLevels { get; set; } = new List<string>()
        {
            "always_trusted"
        };
    }



}
