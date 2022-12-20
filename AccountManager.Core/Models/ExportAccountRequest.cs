namespace AccountManager.Core.Models
{
    public class ExportAccountRequest
    {
        private const string DefaultFileName = "export.omni";
        public List<Account> Accounts { get; set; } = new();
        public string Password { get; set; } = "";
        public string FilePath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), DefaultFileName);
    }
}