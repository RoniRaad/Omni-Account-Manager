using System.Reflection;
using WindowsShortcutFactory;

namespace AccountManager.Infrastructure.Services.FileSystem
{
    public class ShortcutService : IShortcutService
    {
        public void CreateDesktopLoginShortcut(string name, Guid accountGuid, string iconPath)
        {
            WindowsShortcut newShortcut = new()
            {
                Path = $@"{Assembly.GetEntryAssembly()?.Location.Replace("dll", ".exe")}",
                Arguments = $"/login {accountGuid}",
                IconLocation = iconPath
            };

            newShortcut.Save($@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\{name}.lnk");
        }
    }
}
