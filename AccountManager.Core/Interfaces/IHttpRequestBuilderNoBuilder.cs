namespace AccountManager.Infrastructure.Clients
{
    public interface IHttpRequestBuilder
    {
        IHttpRequestBuilderInitialize CreateBuilder();
        IHttpRequestBuilderReadyToExecute CreateBuilder(string uri);
    }
}