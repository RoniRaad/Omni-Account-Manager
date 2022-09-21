namespace AccountManager.Core.Models.Steam
{
    public class LibraryFoldersWrapper
    {
        public Dictionary<string, LibraryFolder>? LibraryFolders { get; set; }
    }

    public class LibraryFolder
    {
        public string Path { get; set; } = string.Empty;
    }
}
