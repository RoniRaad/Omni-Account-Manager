using System.Net;

namespace AccountManager.Infrastructure.Clients
{
    public interface IHttpRequestBuilderInitialize
    {
        IHttpRequestBuilderReadyToExecute SetUri(string destination);
    }

    public interface IHttpRequestBuilderReadyToExecute
    {
        IHttpRequestBuilderReadyToExecute AddCookie(string name, string value);
        IHttpRequestBuilderReadyToExecute AddCookie(string cookieHeader);
        IHttpRequestBuilderReadyToExecute AddCookie(Cookie cookie);
        IHttpRequestBuilderReadyToExecute AddCookies(CookieCollection cookie);
        IHttpRequestBuilderReadyToExecute AddHeader(string name, string value);
        IHttpRequestBuilderReadyToExecute AddHeaders(string name, string[] value);
        IHttpRequestBuilderReadyToExecute SetUserAgent(string userAgent);
        IHttpRequestBuilderReadyToExecute SetBearerToken(string token);
        IHttpRequestBuilderReadyToExecute SetContent<T>(T content);
        Task<IHttpRequestBuilderResponse<string>> Delete();
        Task<IHttpRequestBuilderResponse<T>> Delete<T>() where T : new();
        Task<IHttpRequestBuilderResponse<string>> Get();
        Task<IHttpRequestBuilderResponse<T>> Get<T>() where T : new();
        Task<IHttpRequestBuilderResponse<string>> Post();
        Task<IHttpRequestBuilderResponse<T>> Post<T>() where T : new(); 
        Task<IHttpRequestBuilderResponse<string>> Put();
        Task<IHttpRequestBuilderResponse<T>> Put<T>()where T : new();
    }
}