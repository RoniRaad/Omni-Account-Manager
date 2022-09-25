namespace AccountManager.Infrastructure.Services
{
    public sealed class IpcReceivedEventArgs : EventArgs
    {
        public string? MethodName { get; set; }
        public string? Json { get; set; }
    }
}
