using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using System.Diagnostics;
using AccountManager.Core.Enums;
using AccountManager.Core.Factories;
using AccountManager.Infrastructure.Services.FileSystem;
using AccountManager.Core.Services;
using AccountManager.Core.Models.RiotGames.Requests;
using Microsoft.Extensions.Caching.Memory;

namespace AccountManager.Infrastructure.Services.Platform
{
    public class TeamFightTacticsPlatformService : IPlatformService
    {
        private readonly ILeagueClient _leagueClient;
        private readonly IRiotClient _riotClient;
        private readonly RiotFileSystemService _riotFileSystemService;
        private readonly AlertService _alertService;
        private readonly IMemoryCache _memoryCache;
        private readonly Dictionary<string, string> RankColorMap = new Dictionary<string, string>()
        {
            {"iron", "#000000"},
            {"bronze", "#ac3d14"},
            {"silver", "#7e878b"},
            {"gold", "#FFD700"},
            {"platinum", "#25cb6e"},
            {"diamond", "#9e7ad6"},
            {"master", "#f359f9"},
            {"grandmaster", "#f8848f"},
            {"challenger", "#4ee1ff"},
        };
        public TeamFightTacticsPlatformService(ILeagueClient leagueClient, IRiotClient riotClient, 
            RiotFileSystemService riotFileSystemService, AlertService alertService, IMemoryCache memoryCache)
        {
            _leagueClient = leagueClient;
            _riotClient = riotClient;
            _riotFileSystemService = riotFileSystemService;
            _alertService = alertService;
            _memoryCache = memoryCache;
        }

        public async Task Login(Account account)
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

                await _riotFileSystemService.WriteRiotYaml("NA", authResponse?.Cookies?.Tdid?.Value ?? "", authResponse?.Cookies?.Ssid?.Value ?? "",
                    authResponse?.Cookies?.Sub?.Value ?? "", authResponse?.Cookies?.Csid?.Value ?? "");

                StartLeague();
            }
            catch
            {
                _alertService.AddErrorMessage("There was an error signing in.");
            }
        }

        private void StartLeague()
        {
            var startLeagueCommandline = "--launch-product=league_of_legends --launch-patchline=live";
            var startLeague = new ProcessStartInfo
            {
                FileName = GetRiotExePath(),
                Arguments = startLeagueCommandline
            };
            Process.Start(startLeague);
        }

        public async Task<(bool, Rank)> TryFetchRank(Account account)
        {
            var rankCacheString = $"{account.Username}.teamfighttactics.rank";
            if (_memoryCache.TryGetValue(rankCacheString, out Rank? rank) && rank is not null)
                return (true, rank);

            rank = new Rank();
            try
            {
                if (string.IsNullOrEmpty(account.PlatformId))
                    account.PlatformId = await _riotClient.GetPuuId(account.Username, account.Password);
                if (string.IsNullOrEmpty(account.PlatformId))
                    return (false, rank);

                rank = await _leagueClient.GetTFTRankByPuuidAsync(account);

                SetRankColor(rank);

                if (!string.IsNullOrEmpty(rank?.Tier))
                    _memoryCache.Set(rankCacheString, rank, TimeSpan.FromHours(1));

                if (rank is null)
                    return (false, new());

                return (true, rank);
            }
            catch
            {
                return (false, new Rank());
            }
        }


        public async Task<(bool, string)> TryFetchId(Account account)
        {
            try
            {
                if (!string.IsNullOrEmpty(account.PlatformId))
                    return (true, account.PlatformId);

                var id = await _riotClient.GetPuuId(account.Username, account.Password);
                return (id is not null, id ?? string.Empty);
            }
            catch
            {
                return (false, string.Empty);
            }
        }

        private void SetRankColor(Rank rank)
        {
            if (rank.Tier is null)
                return;

            rank.Color = RankColorMap.FirstOrDefault((kvp) => rank.Tier.ToLower().Equals(kvp.Key)).Value;
        }

        private DriveInfo? FindRiotDrive()
        {
            DriveInfo? riotDrive = DriveInfo.GetDrives().FirstOrDefault(
                (drive) => Directory.Exists($"{drive?.RootDirectory}\\Riot Games"), null);

            return riotDrive;
        }

        private string GetRiotExePath()
        {
            return @$"{FindRiotDrive()?.RootDirectory}\Riot Games\Riot Client\RiotClientServices.exe";
        }

        public async Task<(bool, List<RankedGraphData>)> TryFetchRankedGraphData(Account account)
        {
            return (true, await Task.FromResult(new List<RankedGraphData>()));
        }
    }
}
