using AccountManager.Core.Models.RiotGames;

namespace AccountManager.Infrastructure.Clients
{
    public class RiotAuthTokensResponse
    {
        public string AccessToken { get; set; } = "";
        public string IdToken { get; set; } = "";
        public int ExpiresIn { get; set; } = 0;

        public RiotAuthCookies? Cookies { get; set; }
    }
}
