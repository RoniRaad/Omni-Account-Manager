using System.Net;

namespace AccountManager.Infrastructure.Clients
{
    public interface IHttpRequestBuilderResponse<T>
    {
        CookieCollection? Cookies { get; set; }
        Dictionary<string, string>? Headers { get; set; }
        string? Location { get; set; }
        T? ResponseContent { get; set; }
        HttpStatusCode StatusCode { get; set; }
    }
}