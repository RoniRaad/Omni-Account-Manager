namespace AccountManager.Infrastructure.Services.FileSystem
{
    public interface IShortcutService
    {
        void CreateDesktopLoginShortcut(string name, Guid accountGuid, string iconPath);
    }
}