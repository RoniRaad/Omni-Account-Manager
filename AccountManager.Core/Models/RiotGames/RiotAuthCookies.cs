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
                var trimmedCookie = cookie.Substring(0, cookie.IndexOf(";"));
                if (trimmedCookie.StartsWith("tdid"))
                    Tdid = trimmedCookie;
                if (trimmedCookie.StartsWith("ssid"))
                    Ssid = trimmedCookie;
                if (trimmedCookie.StartsWith("sub"))
                    Sub = trimmedCookie;
                if (trimmedCookie.StartsWith("csid"))
                    Csid = trimmedCookie;
                if (trimmedCookie.StartsWith("clid"))
                    Clid = trimmedCookie;
                if (trimmedCookie.StartsWith("asid"))
                    Asid = trimmedCookie;
                if (trimmedCookie.StartsWith("__cf"))
                    CloudFlare = trimmedCookie;
            }
        }

        public string GetCookieHeader()
        {
            var header = string.Empty;
            if (!string.IsNullOrEmpty(Tdid))
                header += $"{Tdid};";
            if (!string.IsNullOrEmpty(Ssid))
                header += $"{Ssid};";
            if (!string.IsNullOrEmpty(Sub))
                header += $"{Sub};";
            if (!string.IsNullOrEmpty(Csid))
                header += $"{Csid};";
            if (!string.IsNullOrEmpty(Clid))
                header += $"{Clid};";
            if (!string.IsNullOrEmpty(Asid))
                header += $"{Asid};";
            if (!string.IsNullOrEmpty(CloudFlare))
                header += $"{CloudFlare};";

            return header;
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
