using AccountManager.Core.Models.RiotGames.Valorant.Responses;
using CliWrap;
using CliWrap.Buffered;
using CliWrap.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AccountManager.Infrastructure.Clients
{
    public class CurlRequestBuilder
    {
        private string uri = "";
        private Command cliWrapper =  CliWrap.Cli.Wrap("curl");
        ArgumentsBuilder argumentsBuilder = new ArgumentsBuilder();
        public CurlRequestBuilder SetUri(string destination)
        {
            uri = destination;
            return this;
        }

        public CurlRequestBuilder AddHeader(string name, string value)
        {
            argumentsBuilder.Add("-H").Add($"{name}: {value}");
            return this;
        }

        public CurlRequestBuilder AddHeader(string name, string[] value)
        {
            argumentsBuilder.Add("-H").Add($"{name}: {string.Join("; ", value)}");
            return this;
        }

        public CurlRequestBuilder AddContent<T>(T content)
        {
            argumentsBuilder.Add("-d").Add(JsonSerializer.Serialize(content));
            return this;
        }
        public CurlResponse<T> Execute<T>() where T : new()
        {
            argumentsBuilder.Add($"{uri}");
            var response = cliWrapper.WithArguments(argumentsBuilder.Build())
            .WithValidation(CliWrap.CommandResultValidation.None)
            .ExecuteBufferedAsync();

            return new CurlResponse<T>
            {
                ResponseContent = new(),
                Headers = new()
            };
        }

        public class CurlResponse<T>
        {
            public Dictionary<string, string> Headers { get; set; }
            public T ResponseContent { get; set; }
        }
        /**public async Task<TResponse?> PutAsync<TResponse, TContent>(string uri, TContent requestContent, Dictionary<string, string> headers)
        {
            var response = await CliWrap.Cli.Wrap("curl")
                .WithArguments((builder) => builder
                .Add("-i -X PUT", false)
                .Add("-H").Add("Content-Type: application/json")
                .Add("-H").Add($"User-Agent: RiotClient/50.0.0.4396195.4381201 rso-auth (Windows;10;;Professional, x64)")
                .Add("-d").Add(JsonSerializer.Serialize(requestContent))
                .Add($"{uri}"))
                .WithValidation(CliWrap.CommandResultValidation.None)
                .ExecuteBufferedAsync();

            return JsonSerializer.Deserialize<TResponse>(response?.StandardOutput ?? "");
        }
        public async Task<TResponse?> GetAsync<TResponse, TContent>(string uri, TContent requestContent, Dictionary<string, string> headers)
        {
            var response = await CliWrap.Cli.Wrap("curl")
                .WithArguments((builder) => builder
                .Add("-i -X GET", false)
                .Add("-H").Add("Content-Type: application/json")
                .Add("-H").Add($"User-Agent: RiotClient/50.0.0.4396195.4381201 rso-auth (Windows;10;;Professional, x64)")
                .Add("-d").Add(JsonSerializer.Serialize(requestContent))
                .Add($"{uri}"))
                .WithValidation(CliWrap.CommandResultValidation.None)
                .ExecuteBufferedAsync();

            return JsonSerializer.Deserialize<TResponse>(response?.StandardOutput ?? "");
        }
        public async Task<TResponse?> PostAsync<TResponse, TContent>(string uri, TContent requestContent, Dictionary<string, string> headers)
        {
            var response = await CliWrap.Cli.Wrap("curl")
                .WithArguments((builder) => builder
                .Add("-i -X POST", false)
                .Add("-H").Add("Content-Type: application/json")
                .Add("-H").Add($"User-Agent: RiotClient/50.0.0.4396195.4381201 rso-auth (Windows;10;;Professional, x64)")
                .Add("-d").Add(JsonSerializer.Serialize(requestContent))
                .Add($"{uri}/api/v1/authorization"))
                .WithValidation(CliWrap.CommandResultValidation.None)
                .ExecuteBufferedAsync();

            return JsonSerializer.Deserialize<TResponse>(response?.StandardOutput ?? "");
        }
        public async Task<TResponse?> DeleteAsync<TResponse, TContent>(string uri, TContent requestContent, Dictionary<string, string> headers)
        {
            var response = await CliWrap.Cli.Wrap("curl")
                .WithArguments((builder) => {
                    builder
                    .Add("-i -X PUT", false);
                    foreach (var kvp in headers)
                    {
                        builder.Add("-H").Add($"{kvp.Value}: {kvp.Value}");
                    }
                    if (requestContent is not null)
                    {
                        builder.Add("-d").Add(JsonSerializer.Serialize(requestContent));
                    }
                    
                    builder.Add($"{uri}/api/v1/authorization");
                })
                .WithValidation(CliWrap.CommandResultValidation.None)
                .ExecuteBufferedAsync();

            return JsonSerializer.Deserialize<TResponse>(response?.StandardOutput ?? "");
        }**/
    }
}
