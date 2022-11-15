namespace AccountManager.Core.Models
{
    public class ExportAccountRequest
    {
        public List<Account> Accounts { get; set; } = new();
        public string Password { get; set; } = "";
        public string FilePath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    }
}