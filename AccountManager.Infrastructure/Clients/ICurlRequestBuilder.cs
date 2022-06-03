using System.Net;

namespace AccountManager.Infrastructure.Clients
{
    public interface ICurlRequestBuilderInitialize
    {
        ICurlRequestBuilderReadyToExecute SetUri(string destination);
    }

    public interface ICurlRequestBuilderReadyToExecute
    {
        ICurlRequestBuilderReadyToExecute AddCookie(string name, string value);
        ICurlRequestBuilderReadyToExecute AddCookie(string cookieHeader);
        ICurlRequestBuilderReadyToExecute AddCookie(Cookie cookie);
        ICurlRequestBuilderReadyToExecute AddCookies(CookieCollection cookie);
        ICurlRequestBuilderReadyToExecute AddHeader(string name, string value);
        ICurlRequestBuilderReadyToExecute AddHeaders(string name, string[] value);
        ICurlRequestBuilderReadyToExecute SetUserAgent(string userAgent);
        ICurlRequestBuilderReadyToExecute SetBearerToken(string token);
        ICurlRequestBuilderReadyToExecute SetContent<T>(T content);
        Task<CurlRequestBuilder.CurlResponse<string>> Delete();
        Task<CurlRequestBuilder.CurlResponse<T>> Delete<T>() where T : new();
        Task<CurlRequestBuilder.CurlResponse<string>> Get();
        Task<CurlRequestBuilder.CurlResponse<T>> Get<T>() where T : new();
        Task<CurlRequestBuilder.CurlResponse<string>> Post();
        Task<CurlRequestBuilder.CurlResponse<T>> Post<T>() where T : new(); 
        Task<CurlRequestBuilder.CurlResponse<string>> Put();
        Task<CurlRequestBuilder.CurlResponse<T>> Put<T>()where T : new();
    }
}