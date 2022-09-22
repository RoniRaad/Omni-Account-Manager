namespace AccountManager.Core.Models.Steam
{
    public sealed class LibraryFoldersWrapper
    {
        public Dictionary<string, LibraryFolder>? LibraryFolders { get; set; }
    }

    public sealed class LibraryFolder
    {
        public string Path { get; set; } = string.Empty;
    }
}
