using AccountManager.Core.Models.RiotGames.Valorant;
using System.Net;

namespace AccountManager.Core.Models.RiotGames
{
    public sealed class RiotAuthResponse
    {
        public TokenResponseWrapper? Content { get; set; }
        public RiotAuthCookies? Cookies { get; set; }
    }

    public sealed class RiotAuthCookies
    {
        public RiotAuthCookies() { }

        public RiotAuthCookies(CookieCollection cookies)
        {
            if (cookies is null)
                return;

            foreach (var cookie in cookies.ToList())
            {
                if (cookie.Name.ToLower() == "tdid")
                    Tdid = cookie;
                if (cookie.Name.ToLower() == "ssid")
                    Ssid = cookie;
                if (cookie.Name.ToLower() == "sub")
                    Sub = cookie;
                if (cookie.Name.ToLower() == "csid")
                    Csid = cookie;
                if (cookie.Name.ToLower() == "clid")
                    Clid = cookie;
                if (cookie.Name.ToLower() == "asid")
                    Asid = cookie;
                if (cookie.Name.ToLower() == "__cf")
                    CloudFlare = cookie;
            }
        }

        public CookieCollection GetCookies()
        {
            var cookieList = new CookieCollection();

            if (Asid is not null)
                cookieList.Add(Asid);

            if (Tdid is not null)
                cookieList.Add(Tdid);

            if (CloudFlare is not null)
                cookieList.Add(CloudFlare);

            if (Clid is not null)
                cookieList.Add(Clid);

            if (Ssid is not null)
                cookieList.Add(Ssid);

            if (Sub is not null)
                cookieList.Add(Sub);

            if (Csid is not null)
                cookieList.Add(Csid);

            return cookieList;
        }

        public Cookie? Tdid { get; set; }
        public Cookie? Ssid { get; set; }
        public Cookie? Sub { get; set; }
        public Cookie? Csid { get; set; }
        public Cookie? Clid { get; set; }
        public Cookie? Asid { get; set; }
        public Cookie? CloudFlare { get; set; }
    }
}
