namespace AccountManager.Infrastructure.Clients
{
    public interface ICurlRequestBuilder
    {
        IHttpRequestBuilderInitialize CreateBuilder();
        IHttpRequestBuilderReadyToExecute CreateBuilder(string uri);
    }
}