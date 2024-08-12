using AccountManager.Core.Static;
using Microsoft.Extensions.Caching.Distributed;
using System.Net;

namespace AccountManager.Infrastructure.Clients
{
	public class RiotCurlRequestBuilder : CurlRequestBuilder
	{
		private const string tdidCacheKey = $"riot.auth.tdid";
		private readonly IDistributedCache _persistantCache;
		private static DateTimeOffset lastFailure;
		private static int failures;
		private const int timeoutMinutes = 60;

		public RiotCurlRequestBuilder(IDistributedCache persistantCache)
		{
			_persistantCache = persistantCache;
			AddPreRequestMiddleware(AddTdidToCookiesMiddleware);
			AddPreRequestMiddleware(ThrottleRequestsWhenTooManyFailures);
			AddPostRequestMiddleware(PostRequestMiddleware);
			AddPostRequestMiddleware(HandleErroredRequest);
		}

		public override IHttpRequestBuilderInitialize CreateBuilder()
		{
			return new RiotCurlRequestBuilder(_persistantCache);
		}

		public override IHttpRequestBuilderReadyToExecute CreateBuilder(string uri)
		{
			var builder = new RiotCurlRequestBuilder(_persistantCache);
			return builder.SetUri(uri);
		}

		private async Task<bool> AddTdidToCookiesMiddleware(string uriString, CookieContainer requestCookies)
		{
			var uri = new Uri(uriString);
			var tdidCookie = await _persistantCache.GetAsync<Cookie>(tdidCacheKey);

			if (tdidCookie is not null)
				requestCookies.Add(tdidCookie);

			var cookieHeader = requestCookies.GetCookieHeader(uri);
			if (!cookieHeader.Contains("tdid"))
			{
				if (!string.IsNullOrEmpty(cookieHeader))
					cookieHeader += $";tdid={Guid.NewGuid()}";
				else
					cookieHeader = $"tdid={Guid.NewGuid()}";
			}

			requestCookies.SetCookies(uri, cookieHeader);

			return true;
		}

		private async Task<bool> ThrottleRequestsWhenTooManyFailures(string uriString, CookieContainer requestCookies)
		{
			if (failures > 10 && lastFailure.AddMinutes(timeoutMinutes) < DateTimeOffset.Now)
			{
				// Causes the request to fail to prevent throttling...
				return false;
			}

			return true;
		}

		private async Task PostRequestMiddleware(string uriString, CookieContainer requestCookies, string requestContentString, CurlResponse<string> curlResponse)
		{
			var responseCookieCollection = curlResponse.Cookies;
			var tdidResponseCookie = responseCookieCollection?.FirstOrDefault((cookie) => cookie?.Name?.ToLower() == "tdid", null);

			if (tdidResponseCookie is not null)
				await _persistantCache.SetAsync(tdidCacheKey, tdidResponseCookie);
		}

		private Task HandleErroredRequest(string uriString, CookieContainer requestCookies, string requestContentString, CurlResponse<string> curlResponse)
		{
			var statusCodeInt = (int)curlResponse.StatusCode;
			if (statusCodeInt < 200 || statusCodeInt >= 399)
			{
				failures++;
				lastFailure = DateTimeOffset.Now;
			}

			return Task.CompletedTask;
		}
	}
}
