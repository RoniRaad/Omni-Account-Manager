namespace AccountManager.Infrastructure.Services.FileSystem
{
    public interface IShortcutService
    {
        bool TryCreateDesktopLoginShortcut(string name, Guid accountGuid, string iconPath);
    }
}