using System.Reflection;
using WindowsShortcutFactory;

namespace AccountManager.Infrastructure.Services.FileSystem
{
    public sealed class ShortcutService : IShortcutService
    {
        public bool TryCreateDesktopLoginShortcut(string name, Guid accountGuid, string iconPath)
        {
            try
            {
                WindowsShortcut newShortcut = new()
                {
                    Path = $@"{Assembly.GetEntryAssembly()?.Location.Replace("dll", "exe")}",
                    Arguments = $"/login {accountGuid}",
                    IconLocation = iconPath
                };

                newShortcut.Save($@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\{name}.lnk");
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
