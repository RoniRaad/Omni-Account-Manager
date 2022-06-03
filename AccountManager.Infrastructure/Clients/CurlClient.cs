using AccountManager.Core.Static;
using CliWrap;
using CliWrap.Buffered;
using CliWrap.Builders;
using Microsoft.Extensions.Caching.Distributed;
using System.Net;
using System.Text.Json;

namespace AccountManager.Infrastructure.Clients
{
    public class CurlRequestBuilder : ICurlRequestBuilder, ICurlRequestBuilderInitialize, ICurlRequestBuilderReadyToExecute
    {
        private string uri = "";
        private Command cliWrapper = Cli.Wrap("curl");
        CookieContainer requestCookies = new();
        ArgumentsBuilder argumentsBuilder = new();
        IDistributedCache _persistantCache; 
        public CurlRequestBuilder(IDistributedCache persistantCache) 
        {
            _persistantCache = persistantCache;
        }

        public ICurlRequestBuilderInitialize CreateBuilder()
        {
            return new CurlRequestBuilder(_persistantCache);
        }

        public ICurlRequestBuilderReadyToExecute CreateBuilder(string uri)
        {
            var builder = new CurlRequestBuilder(_persistantCache);
            return builder.SetUri(uri);
        }

        public ICurlRequestBuilderReadyToExecute SetUri(string destination)
        {
            uri = destination;
            return this;
        }

        public ICurlRequestBuilderReadyToExecute SetUserAgent(string userAgent)
        {
            argumentsBuilder.Add("-H").Add($"User-Agent: {userAgent}");
            return this;
        }

        public ICurlRequestBuilderReadyToExecute AddHeader(string name, string value)
        {
            argumentsBuilder.Add("-H").Add($"{name}: {value}");
            return this;
        }
        public ICurlRequestBuilderReadyToExecute SetBearerToken(string token)
        {
            argumentsBuilder.Add("-H").Add($"Authorization: Bearer {token}");
            return this;
        }

        public ICurlRequestBuilderReadyToExecute AddHeaders(string name, string[] value)
        {
            argumentsBuilder.Add("-H").Add($"{name}: {string.Join("; ", value)}");
            return this;
        }

        public ICurlRequestBuilderReadyToExecute AddCookie(Cookie cookie)
        {
            requestCookies.Add(cookie);
            return this;
        }

        public ICurlRequestBuilderReadyToExecute AddCookie(string cookieHeader)
        {
            var cookieContainer = new CookieContainer();
            cookieContainer.SetCookies(new Uri(uri), cookieHeader);
            requestCookies.Add(cookieContainer.GetAllCookies());

            return this;
        }

        public ICurlRequestBuilderReadyToExecute AddCookie(string name, string value)
        {
            var cookie = new Cookie(name, value);
            requestCookies.Add(cookie);

            return this;
        }

        public ICurlRequestBuilderReadyToExecute AddCookies(CookieCollection cookie)
        {
            requestCookies.Add(cookie);
            return this;
        }
        public ICurlRequestBuilderReadyToExecute SetContent<T>(T content)
        {
            argumentsBuilder.Add("-H").Add("Content-Type: application/json");
            argumentsBuilder.Add("-d").Add(JsonSerializer.Serialize(content));
            return this;
        }

        public async Task<CurlResponse<string>> ExecuteAsync()
        {
            var tdidCacheKey = $"riot.auth.tdid";
            var tdidCookie = await _persistantCache.GetAsync<Cookie>(tdidCacheKey);
            var cookieContainer = new CookieContainer();

            if (tdidCookie is not null)
                requestCookies.Add(tdidCookie);

            var cookieHeader = requestCookies.GetCookieHeader(new Uri(uri));
            argumentsBuilder.Add("-H").Add($"Cookie: {cookieHeader}");

            argumentsBuilder.Add($"{uri}");
            var argumentsString = argumentsBuilder.Build();
            var response = await cliWrapper.WithArguments(argumentsString)
            .WithValidation(CliWrap.CommandResultValidation.None)
            .ExecuteBufferedAsync();
            var responseLines = response.StandardOutput.Split("\n");
            var cookieHeaders = responseLines.Where((header) => header.ToLower().StartsWith("set-cookie"))
                .Select((cookieHeader) => cookieHeader
                .Substring(cookieHeader.ToLower().IndexOf("set-cookie:")));

            int.TryParse(responseLines[0].Split(" ")[1], out var statusCode);
            var responseJson = responseLines[^1];

            foreach (var responseCookieHeader in cookieHeaders)
            {
                var trimmedCookieHeader = responseCookieHeader[12..];
                var tempContainer = new CookieContainer();
                tempContainer.SetCookies(new Uri(uri), trimmedCookieHeader);
                cookieContainer.Add(tempContainer.GetAllCookies());
            }

            var responseCookieCollection = cookieContainer.GetAllCookies();
            var tdidResponseCookie = responseCookieCollection.FirstOrDefault((cookie) => cookie?.Name?.ToLower() == "tdid", null);

            if (tdidResponseCookie is not null)
                await _persistantCache.SetAsync(tdidCacheKey, tdidResponseCookie);
            
            return new CurlResponse<string>
            {
                ResponseContent = responseJson,
                Headers = new(),
                StatusCode = (HttpStatusCode)statusCode,
                Cookies = cookieContainer.GetAllCookies()
            };
        }

        public async Task<CurlResponse<T>> ExecuteAsync<T>() where T : new()
        {
            var stringResponse = await ExecuteAsync();

            return new CurlResponse<T>()
            {
                Cookies = stringResponse.Cookies,
                Headers = stringResponse.Headers,
                ResponseContent = JsonSerializer.Deserialize<T>(stringResponse?.ResponseContent ?? "{}"),
                StatusCode = stringResponse?.StatusCode ?? HttpStatusCode.BadRequest
            };
        }

        public async Task<CurlResponse<string>> Delete()
        {
            argumentsBuilder.Add("-i -X DELETE", false);

            return await ExecuteAsync();
        }

        public async Task<CurlResponse<string>> Get()
        {
            argumentsBuilder.Add("-i -X GET", false);

            return await ExecuteAsync();
        }

        public async Task<CurlResponse<string>> Post()
        {
            argumentsBuilder.Add("-i -X POST", false);

            return await ExecuteAsync();
        }

        public async Task<CurlResponse<string>> Put()
        {
            argumentsBuilder.Add("-i -X PUT", false);

            return await ExecuteAsync();
        }

        public async Task<CurlResponse<T>> Delete<T>() where T : new()
        {
            argumentsBuilder.Add("-i -X DELETE", false);

            return await ExecuteAsync<T>();
        }

        public async Task<CurlResponse<T>> Get<T>() where T : new()
        {
            argumentsBuilder.Add("-i -X GET", false);

            return await ExecuteAsync<T>();
        }

        public async Task<CurlResponse<T>> Post<T>() where T : new()
        {
            argumentsBuilder.Add("-i -X POST", false);

            return await ExecuteAsync<T>();
        }

        public async Task<CurlResponse<T>> Put<T>() where T : new()
        {
            argumentsBuilder.Add("-i -X PUT", false);

            return await ExecuteAsync<T>();
        }

        public class CurlResponse
        {
            public HttpStatusCode StatusCode { get; set; }
            public Dictionary<string, string>? Headers { get; set; }
            public CookieCollection? Cookies { get; set; }
            public string? ResponseContent { get; set; }

        }

        public class CurlResponse<T>
        {
            public HttpStatusCode StatusCode { get; set; }
            public Dictionary<string, string>? Headers { get; set; }
            public CookieCollection? Cookies { get; set; }
            public T? ResponseContent { get; set; }
        }
    }
}