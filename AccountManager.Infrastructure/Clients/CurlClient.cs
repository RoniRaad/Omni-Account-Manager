using AccountManager.Core.Static;
using CliWrap;
using CliWrap.Buffered;
using CliWrap.Builders;
using Microsoft.Extensions.Caching.Distributed;
using System.Net;
using System.Reflection;
using System.Text.Json;

namespace AccountManager.Infrastructure.Clients
{
    public sealed class CurlRequestBuilder : IHttpRequestBuilder, IHttpRequestBuilderInitialize, IHttpRequestBuilderReadyToExecute
    {
        private string uri = "";
        private readonly Command _cliWrapper = Cli.Wrap(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? ".","curl","curl.exe"))
            .WithWorkingDirectory(Directory.GetCurrentDirectory());
        private readonly CookieContainer _requestCookies = new();
        private readonly ArgumentsBuilder _argumentsBuilder = new();
        private readonly IDistributedCache _persistantCache; 
        public CurlRequestBuilder(IDistributedCache persistantCache) 
        {
            _persistantCache = persistantCache;
            _argumentsBuilder.Add("--tlsv1.3")
                .Add("--tls13-ciphers")
                .Add("TLS_CHACHA20_POLY1305_SHA256:TLS_AES_128_GCM_SHA256:TLS_AES_256_GCM_SHA384:TLS_ECDHE_ECDSA_WITH_CHACHA20_POLY1305_SHA256")
                .Add("-H")
                .Add("Accept-Language: *");
        }

        public IHttpRequestBuilderInitialize CreateBuilder()
        {
            return new CurlRequestBuilder(_persistantCache);
        }

        public IHttpRequestBuilderReadyToExecute CreateBuilder(string uri)
        {
            var builder = new CurlRequestBuilder(_persistantCache);
            return builder.SetUri(uri);
        }

        public IHttpRequestBuilderReadyToExecute SetUri(string destination)
        {
            uri = destination;
            return this;
        }

        public IHttpRequestBuilderReadyToExecute SetUserAgent(string userAgent)
        {
            _argumentsBuilder.Add("-H").Add($"User-Agent: {userAgent}");
            return this;
        }

        public IHttpRequestBuilderReadyToExecute AddHeader(string name, string value)
        {
            _argumentsBuilder.Add("-H").Add($"{name}: {value}");
            return this;
        }
        public IHttpRequestBuilderReadyToExecute SetBearerToken(string token)
        {
            _argumentsBuilder.Add("-H").Add($"Authorization: Bearer {token}");
            return this;
        }

        public IHttpRequestBuilderReadyToExecute AddHeaders(string name, string[] value)
        {
            _argumentsBuilder.Add("-H").Add($"{name}: {string.Join("; ", value)}");
            return this;
        }

        public IHttpRequestBuilderReadyToExecute AddCookie(Cookie cookie)
        {
            _requestCookies.Add(cookie);
            return this;
        }

        public IHttpRequestBuilderReadyToExecute AddCookie(string cookieHeader)
        {
            var cookieContainer = new CookieContainer();
            cookieContainer.SetCookies(new Uri(uri), cookieHeader);
            _requestCookies.Add(cookieContainer.GetAllCookies());

            return this;
        }

        public IHttpRequestBuilderReadyToExecute AddCookie(string name, string value)
        {
            var cookie = new Cookie(name, value);
            _requestCookies.Add(cookie);

            return this;
        }

        public IHttpRequestBuilderReadyToExecute AddCookies(CookieCollection cookie)
        {
            _requestCookies.Add(cookie);
            return this;
        }
        public IHttpRequestBuilderReadyToExecute SetContent<T>(T content)
        {
            _argumentsBuilder.Add("-H").Add("Content-Type: application/json");
            _argumentsBuilder.Add("-d").Add(JsonSerializer.Serialize(content));
            return this;
        }

        private CookieContainer ParseCookies(IEnumerable<string> setCookieHeaders)
        {
            var cookieContainer = new CookieContainer();

            foreach (var responseCookieHeader in setCookieHeaders)
            {
                var trimmedCookieHeader = responseCookieHeader[12..];
                var tempContainer = new CookieContainer();
                tempContainer.SetCookies(new Uri(uri), trimmedCookieHeader);
                cookieContainer.Add(tempContainer.GetAllCookies());
            }

            return cookieContainer;
        }

        public async Task<CurlResponse<string>> ExecuteAsync()
        {
            try
            {
                var tdidCacheKey = $"riot.auth.tdid";
                var tdidCookie = await _persistantCache.GetAsync<Cookie>(tdidCacheKey);

                if (tdidCookie is not null)
                    _requestCookies.Add(tdidCookie);

                var cookieHeader = _requestCookies.GetCookieHeader(new Uri(uri));

                if (!cookieHeader.Contains("tdid"))
                {
                    if (!string.IsNullOrEmpty(cookieHeader))
                        cookieHeader += $";tdid={Guid.NewGuid()}";
                    else
                        cookieHeader = $"tdid={Guid.NewGuid()}";
                }



                _argumentsBuilder.Add("-H").Add($"Cookie: {cookieHeader}");
                _argumentsBuilder.Add($"{uri.TrimEnd('\\').TrimEnd('/')}");

                var argumentsString = _argumentsBuilder.Build();

                var response = await _cliWrapper.WithArguments(argumentsString)
                .WithValidation(CliWrap.CommandResultValidation.None)
                .ExecuteBufferedAsync();

                var responseLines = response.StandardOutput.Split("\n");
                var cookieHeaders = responseLines.Where((header) => header.ToLower().StartsWith("set-cookie"))
                    .Select((cookieHeader) => cookieHeader[cookieHeader.ToLower().IndexOf("set-cookie:")..]);

                var locationHeader = responseLines?.FirstOrDefault((header) => header?.ToLower()?.StartsWith("location") is true, null);
                var locationValue = locationHeader?.Replace("location:", "").Trim();

                if (!int.TryParse(responseLines?.ElementAtOrDefault(0)?.Split(" ")?.ElementAtOrDefault(1), out var statusCode))
                    statusCode = 400;

                string? responseJson = null;

                if (responseLines is not null)
                    responseJson = responseLines[^1];

                var cookieContainer = ParseCookies(cookieHeaders);

                var responseCookieCollection = cookieContainer.GetAllCookies();
                var tdidResponseCookie = responseCookieCollection.FirstOrDefault((cookie) => cookie?.Name?.ToLower() == "tdid", null);

                if (tdidResponseCookie is not null)
                    await _persistantCache.SetAsync(tdidCacheKey, tdidResponseCookie);

                return new CurlResponse<string>
                {
                    ResponseContent = responseJson,
                    Headers = new(),
                    StatusCode = (HttpStatusCode)statusCode,
                    Cookies = cookieContainer.GetAllCookies(),
                    Location = locationValue
                };
            }
            catch
            {
                return new CurlResponse<string>
                {
                    ResponseContent = null,
                    Headers = new(),
                    StatusCode = HttpStatusCode.BadRequest,
                    Cookies = null,
                    Location = null
                };
            }
        }

        public async Task<IHttpRequestBuilderResponse<T>> ExecuteAsync<T>() where T : new()
        {
            var stringResponse = await ExecuteAsync();

            return new CurlResponse<T>()
            {
                Cookies = stringResponse.Cookies,
                Headers = stringResponse.Headers,
                ResponseContent = JsonSerializer.Deserialize<T>(stringResponse?.ResponseContent ?? "{}"),
                StatusCode = stringResponse?.StatusCode ?? HttpStatusCode.BadRequest,
                Location = stringResponse?.Location
            };
        }

        public async Task<IHttpRequestBuilderResponse<string>> Delete()
        {
            _argumentsBuilder.Add("-i -X DELETE", false);

            return await ExecuteAsync();
        }

        public async Task<IHttpRequestBuilderResponse<T>> Delete<T>() where T : new()
        {
            _argumentsBuilder.Add("-i -X DELETE", false);

            return await ExecuteAsync<T>();
        }

        public async Task<IHttpRequestBuilderResponse<string>> Get()
        {
            _argumentsBuilder.Add("-i -X GET", false);

            return await ExecuteAsync();
        }
        public async Task<IHttpRequestBuilderResponse<T>> Get<T>() where T : new()
        {
            _argumentsBuilder.Add("-i -X GET", false);

            return await ExecuteAsync<T>();
        }

        public async Task<IHttpRequestBuilderResponse<string>> Post()
        {
            _argumentsBuilder.Add("-i -X POST", false);

            return await ExecuteAsync();
        }

        public async Task<IHttpRequestBuilderResponse<T>> Post<T>() where T : new()
        {
            _argumentsBuilder.Add("-i -X POST", false);

            return await ExecuteAsync<T>();
        }

        public async Task<IHttpRequestBuilderResponse<string>> Put()
        {
            _argumentsBuilder.Add("-i -X PUT", false);

            return await ExecuteAsync();
        }

        public async Task<IHttpRequestBuilderResponse<T>> Put<T>() where T : new()
        {
            _argumentsBuilder.Add("-i -X PUT", false);

            return await ExecuteAsync<T>();
        }

        public sealed class CurlResponse<T> : IHttpRequestBuilderResponse<T>
        {
            public HttpStatusCode StatusCode { get; set; }
            public Dictionary<string, string>? Headers { get; set; }
            public CookieCollection? Cookies { get; set; }
            public T? ResponseContent { get; set; }
            public string? Location { get; set; }
        }
    }
}