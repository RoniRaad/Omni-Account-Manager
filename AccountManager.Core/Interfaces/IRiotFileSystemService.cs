namespace AccountManager.Infrastructure.Services.FileSystem
{
    public interface IRiotFileSystemService
    {
        bool DeleteLockfile();
        Task WaitForClientClose();
        Task WaitForClientInit();
        Task WriteRiotYaml(string region, string tdid, string ssid, string sub, string csid);
    }
}