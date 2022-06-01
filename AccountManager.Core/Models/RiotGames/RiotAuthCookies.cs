using AccountManager.Core.Models.RiotGames.Valorant;
using System.Net;

namespace AccountManager.Core.Models.RiotGames
{
    public class RiotAuthResponse
    {
        public TokenResponseWrapper? Content { get; set; }
        public RiotAuthCookies? Cookies { get; set; }
    }

    public class RiotAuthCookies
    {
        public RiotAuthCookies() { }

        public RiotAuthCookies(IList<KeyValuePair<string, IEnumerable<string>>> headers)
        {
            var cookies = headers.FirstOrDefault(cookie => 
                cookie.Key.ToUpper() == "Set-Cookie".ToUpper()).Value;

            if (cookies is null)
                return;

            foreach (var cookie in cookies)
            {
                if (cookie.StartsWith("tdid"))
                    Tdid = cookie;
                if (cookie.StartsWith("ssid"))
                    Ssid = cookie;
                if (cookie.StartsWith("sub"))
                    Sub = cookie;
                if (cookie.StartsWith("csid"))
                    Csid = cookie;
                if (cookie.StartsWith("clid"))
                    Clid = cookie;
                if (cookie.StartsWith("asid"))
                    Asid = cookie;
                if (cookie.StartsWith("__cf"))
                    CloudFlare = cookie;
            }
        }

        public IEnumerable<string> GetCookies()
        {
            var cookieList = new List<string>();

            // For some reason the order here matters.
            if (!string.IsNullOrEmpty(Asid))
                cookieList.Add(Asid);

            if (!string.IsNullOrEmpty(Tdid))
                cookieList.Add(Tdid);

            if (!string.IsNullOrEmpty(CloudFlare))
                cookieList.Add(CloudFlare);

            if (!string.IsNullOrEmpty(Clid))
                cookieList.Add(Clid);

            if (!string.IsNullOrEmpty(Ssid))
                cookieList.Add(Ssid);

            if (!string.IsNullOrEmpty(Sub))
                cookieList.Add(Sub);

            if (!string.IsNullOrEmpty(Csid))
                cookieList.Add(Csid);

            return cookieList;
        }

        public string? Tdid { get; set; }
        public string? Ssid { get; set; }
        public string? Sub { get; set; }
        public string? Csid { get; set; }
        public string? Clid { get; set; }
        public string? Asid { get; set; }
        public string? CloudFlare { get; set; }
    }
}
