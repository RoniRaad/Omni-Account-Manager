namespace AccountManager.Infrastructure.Services
{
    public interface IIpcService
    {
        event EventHandler<IpcReceivedEventArgs> IpcReceived;
    }
}