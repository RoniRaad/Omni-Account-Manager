using AccountManager.Core.Interfaces;
using AccountManager.Core.Models.AppSettings;
using AccountManager.Core.Models.EpicGames;
using AccountManager.Core.Static;
using AccountManager.UI.Windows;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;


namespace AccountManager.UI.Services
{
    public partial class EpicGamesExternalAuthService : IEpicGamesExternalAuthService
    {
        private readonly EpicGamesApiUri _epicGamesApiUri;
        private readonly IEpicGamesTokenClient _epicGamesTokenClient;
        private readonly IDistributedCache _persistantCache;
        private WebView2Browser? WebView { get; set; }
        private event Action<EpicGamesCredentialsRecievedEventArgs> OnCredentialsRecieved = delegate { };

        public EpicGamesExternalAuthService(IEpicGamesTokenClient epicGamesTokenClient,
            IDistributedCache persistantCache, IOptions<EpicGamesApiUri> epicGamesApiUriOptions)
        {
            _epicGamesTokenClient = epicGamesTokenClient;
            _persistantCache = persistantCache;
            _epicGamesApiUri = epicGamesApiUriOptions.Value;
        }

        private async Task<EpicGamesLoginInfo?> GetEpicGamesCredentials(string xsrfToken, string cookies, string username)
        {
            var exchangeCode = await _epicGamesTokenClient.GetExchangeCode(xsrfToken, cookies);
            if (string.IsNullOrEmpty(exchangeCode))
                return null;

            var accessToken = await _epicGamesTokenClient.GetAccessTokenAsync(exchangeCode);
            if (accessToken?.AccountId is null || accessToken?.AccessToken is null)
                return null;

            var accountInfo = await _epicGamesTokenClient.GetAccountInfo(accessToken.AccessToken, accessToken.AccountId);
            if (accountInfo?.DisplayName is null)
                return null;

            return new EpicGamesLoginInfo
            {
                DisplayName = accountInfo.DisplayName,
                LastName = accountInfo.LastName,
                Name = accountInfo.Name,
                RefreshToken = accessToken.RefreshToken,
                Username = username,
                Id = accountInfo.Id
            };
        }

        private void StartEpicGamesWebBrowser(string username, string password)
        {
            if (WebView is not null)
                return;

            WebView = new WebView2Browser();
            WebView.Title = "Epic Games Browser";
            WebView.Show();
            WebView.webv2.Source = new Uri(_epicGamesApiUri.LoginUri);
            WebView.webv2.CoreWebView2InitializationCompleted += async (_, e) =>
            {
                WebView.webv2.CoreWebView2.CookieManager.DeleteAllCookies();
                var cookies = await _persistantCache.GetAsync<List<Cookie>>($"{username}.EpicGames.Cookies");
                cookies ??= new();
                foreach (var cookie in cookies)
                {
                    if (WebView is null)
                        return;

                    var webv2Cookie = WebView.webv2.CoreWebView2.CookieManager.CreateCookieWithSystemNetCookie(cookie);
                    WebView.webv2.CoreWebView2.CookieManager.AddOrUpdateCookie(webv2Cookie);
                }

                WebView.webv2.CoreWebView2.DOMContentLoaded += async (obj, domEvent) =>
                {
                    await InjectLoginJavascript(username, password);
                };

                WebView.webv2.CoreWebView2.WebResourceResponseReceived += async (e, v) =>
                {
                    EpicGamesLoginInfo? loginInfo = null;

                    if (v?.Request?.Uri?.StartsWith("https://www.epicgames.com/id/api/redirect?") is true)
                    {
                        var currentCookies = await WebView.webv2.CoreWebView2.CookieManager.GetCookiesAsync(null);
                        await _persistantCache.SetAsync($"{username}.EpicGames.Cookies", currentCookies.Select((cookie) => cookie.ToSystemNetCookie()).ToList());
                        var xsrfHeader = v?.Request?.Headers?.FirstOrDefault((header) => header.Key.ToLower() == "x-xsrf-token");
                        var xsrfToken = xsrfHeader?.Value;
                        var cookies = v?.Request?.Headers?.FirstOrDefault((header) => header.Key.ToLower() == "cookie").Value;

                        if (cookies is not null && xsrfToken is not null)
                            loginInfo = await GetEpicGamesCredentials(xsrfToken, cookies, username);

                        if (loginInfo is null)
                        {
                            OnCredentialsRecieved.Invoke(new EpicGamesCredentialsRecievedEventArgs()
                            {
                                EpicGamesLoginInfo = new EpicGamesLoginInfo
                                {
                                    DisplayName = null,
                                    LastName = null,
                                    Name = null,
                                    RefreshToken = null,
                                    Username = username
                                }
                            });

                            return;
                        }

                        OnCredentialsRecieved.Invoke(new EpicGamesCredentialsRecievedEventArgs()
                        {
                            EpicGamesLoginInfo = loginInfo
                        });
                    }
                };
            };
        }

        public async Task<EpicGamesLoginInfo?> TryGetEpicGamesAccessTokens(string username, string password)
        {
            int count = 0;
            StartEpicGamesWebBrowser(username, password);
            EpicGamesLoginInfo? loginInfo = null;
            OnCredentialsRecieved += (e) =>
            {
                if (e?.EpicGamesLoginInfo?.Username == username)
                {
                    loginInfo = e?.EpicGamesLoginInfo;
                }
            };

            while (loginInfo is null && count < 60) // At 120 seconds give up and return null
            {
                await Task.Delay(2000);
                count++;
            }

            CloseBrowser();
            return loginInfo;
        }

        public void CloseBrowser()
        {
            WebView?.Close();
            WebView = null;
        }

        private async Task InjectLoginJavascript(string username, string password)
        {
            await (WebView?.webv2?.CoreWebView2?.ExecuteScriptAsync($"usernameFieldFound = false; passwordFieldFound = false; clickedLogin = false; setInterval(() => {{ if (!usernameFieldFound){{ document.querySelector(\"input[autocomplete='username']\").focus(); document.execCommand('insertText', false, '{username}'); usernameFieldFound = true; }} if (!passwordFieldFound){{ document.querySelector(\"input[autocomplete='current-password']\").focus(); document.execCommand('insertText', false, '{password}'); passwordFieldFound = true; }} if (!clickedLogin){{ if (document.querySelector(\"button[type='submit']:not(:disabled)\") != null){{ document.querySelector(\"button[type='submit']:not(:disabled)\").click(); clickedLogin = true; }} }} }}, 1000);") ?? Task.CompletedTask);
        }
    }
}