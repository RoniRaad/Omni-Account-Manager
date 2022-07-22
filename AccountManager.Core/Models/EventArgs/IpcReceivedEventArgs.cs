namespace AccountManager.Infrastructure.Services
{
    public class IpcReceivedEventArgs : EventArgs
    {
        public string? MethodName { get; set; }
        public string? Json { get; set; }
    }
}
