using AccountManager.Core.Interfaces;
using AccountManager.UI.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AccountManager.UI.Services
{
    public class EpicGamesTokenService : IEpicGamesTokenService
    {
        private readonly IHttpClientFactory? _httpClientFactory;
        private WebView2Browser? WebView { get; set; }

        public EpicGamesTokenService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task TrySignIn(string username, string password)
        {
            if (WebView is not null)
                return;

            WebView = new WebView2Browser();
            WebView.Show();
            WebView.webv2.Source = new Uri("https://www.epicgames.com/id/login/epic");
            WebView.webv2.CoreWebView2InitializationCompleted += (_, e) =>
            {
                WebView.webv2.CoreWebView2.NavigationCompleted += async (obj, domEvent) =>
                {
                    await Task.Delay(3000);
                    await TrySetUsername(username);
                    await TrySetPassword(password);
                    await Task.Delay(1000);
                    await TrySubmit();
                };
                WebView.webv2.CoreWebView2.WebResourceResponseReceived += async (e, v) =>
                {
                    var uri = v?.Request?.Uri;
                    if (uri?.Contains("https://www.epicgames.com/id/api/redirect?") is true)
                    {
                        var xsrfHeader = v?.Request?.Headers?.FirstOrDefault((header) => header.Key.ToLower() == "x-xsrf-token");
                        if (xsrfHeader?.Value != null)
                        {
                            var hdr = xsrfHeader?.Value;
                            HttpClient client = _httpClientFactory.CreateClient();
                            var cookies = await WebView.webv2.CoreWebView2.CookieManager.GetCookiesAsync("https://www.epicgames.com");
                            client.DefaultRequestHeaders.Add("X-XSRF-TOKEN", hdr);
                            client.DefaultRequestHeaders.Add("Cookie", v?.Request?.Headers?.FirstOrDefault((hdr) => hdr.Key.ToLower() == "cookie").Value);
                            var response = await client.PostAsync("https://www.epicgames.com/id/api/exchange/generate", null);
                            var code = await response.Content.ReadFromJsonAsync<ExchangeCodeGenerate>();

                            client.DefaultRequestHeaders.Clear();
                            client.DefaultRequestHeaders.Add("Authorization", "basic MzRhMDJjZjhmNDQxNGUyOWIxNTkyMTg3NmRhMzZmOWE6ZGFhZmJjY2M3Mzc3NDUwMzlkZmZlNTNkOTRmYzc2Y2Y=");
                            var dict = new Dictionary<string, string>()
                            {
                                { "token_type", "eg1"},
                                { "grant_type", "exchange_code"},
                                { "exchange_code", code.ExchangeCode},
                            };
                            var content = new FormUrlEncodedContent(dict);
                            var response2 = await client.PostAsync("https://account-public-service-prod03.ol.epicgames.com/account/api/oauth/token", content);
                            var json = await response2.Content.ReadFromJsonAsync<AccessTokenResponse>();
                        }
                    }
                };
            };
        }

        public void CloseBrowser()
        {
            WebView?.Close();
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

        public class ExchangeCodeGenerate
        {
            [JsonPropertyName("code")]
            public string ExchangeCode { get; set; } = "";
        }
        public class AccessTokenResponse
        {
            [JsonPropertyName("access_token")]
            public string AccessToken { get; set; }

            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }

            [JsonPropertyName("expires_at")]
            public DateTime ExpiresAt { get; set; }

            [JsonPropertyName("token_type")]
            public string TokenType { get; set; }

            [JsonPropertyName("refresh_token")]
            public string RefreshToken { get; set; }


            [JsonPropertyName("refresh_expires")]
            public int RefreshExpires { get; set; }

            [JsonPropertyName("refresh_expires_at")]
            public DateTime RefreshExpiresAt { get; set; }

            [JsonPropertyName("account_id")]
            public string AccountId { get; set; }

            [JsonPropertyName("client_id")]
            public string ClientId { get; set; }

            [JsonPropertyName("internal_client")]
            public bool InternalClient { get; set; }

            [JsonPropertyName("client_service")]
            public string ClientService { get; set; }

            [JsonPropertyName("displayName")]
            public string DisplayName { get; set; }

            [JsonPropertyName("app")]
            public string App { get; set; }

            [JsonPropertyName("in_app_id")]
            public string InAppId { get; set; }

            [JsonPropertyName("device_id")]
            public string DeviceId { get; set; }
        }
    }
}