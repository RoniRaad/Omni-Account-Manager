using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.UI.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;


namespace AccountManager.UI.Services
{
    public partial class EpicGamesExternalAuthService : IEpicGamesExternalAuthService
    {
        private static Uri EpicGamesUri = new Uri("https://www.epicgames.com/id/login/epic");
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IEpicGamesTokenClient _epicGamesTokenClient;
        private WebView2Browser? WebView { get; set; }
        private event Action<EpicGamesCredentialsRecievedEventArgs> OnCredentialsRecieved = delegate { };

        public EpicGamesExternalAuthService(IHttpClientFactory httpClientFactory, IEpicGamesTokenClient epicGamesTokenClient)
        {
            _httpClientFactory = httpClientFactory;
            _epicGamesTokenClient = epicGamesTokenClient;
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
            WebView.webv2.Source = EpicGamesUri;
            WebView.webv2.CoreWebView2InitializationCompleted += (_, e) =>
            {

                WebView.webv2.CoreWebView2.DOMContentLoaded += async (obj, domEvent) =>
                {
                    await Task.Delay(500);
                    await TrySetUsername(username);
                    await TrySetPassword(password);
                    await Task.Delay(500);
                    await TrySubmit();
                };

                WebView.webv2.CoreWebView2.WebResourceResponseReceived += async (e, v) =>
                {
                    EpicGamesLoginInfo? loginInfo = null;

                    if (v?.Request?.Uri?.StartsWith("https://www.epicgames.com/id/api/redirect?") is true)
                    {
                        WebView.webv2.CoreWebView2.CookieManager.DeleteAllCookies();
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

        private async Task TrySetUsername(string username)
        {
            await (WebView?.webv2?.CoreWebView2?.ExecuteScriptAsync($"document.querySelector(\"input[autocomplete='username']\").focus();document.execCommand('insertText', false, '{username}');") ?? Task.CompletedTask);
        }

        private async Task TrySetPassword(string password)
        {
            await (WebView?.webv2?.CoreWebView2?.ExecuteScriptAsync($"document.querySelector(\"input[autocomplete='current-password']\").focus();document.execCommand('insertText', false, '{password}');") ?? Task.CompletedTask);
        }

        private async Task TrySubmit()
        {
            await (WebView?.webv2?.CoreWebView2?.ExecuteScriptAsync($"document.querySelector(\"button[type='submit']\").click();") ?? Task.CompletedTask);

        }
    }
}