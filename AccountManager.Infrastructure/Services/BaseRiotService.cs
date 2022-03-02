namespace AccountManager.Infrastructure.Services
{
    public class BaseRiotService
    {
        public string GetCommandLineValue(string commandline, string key)
        {
            key += "=";
            var valueStart = commandline.IndexOf(key) + key.Length;
            var valueEnd = commandline.IndexOf(" ", valueStart);
            return commandline.Substring(valueStart, valueEnd - valueStart).Replace(@"\", "").Replace("\"", "");
        }
    }
}
