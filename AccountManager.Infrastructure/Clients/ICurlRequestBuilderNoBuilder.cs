namespace AccountManager.Infrastructure.Clients
{
    public interface ICurlRequestBuilder
    {
        ICurlRequestBuilderInitialize CreateBuilder();
        ICurlRequestBuilderReadyToExecute CreateBuilder(string uri);
    }
}