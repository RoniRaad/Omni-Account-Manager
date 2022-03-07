namespace AccountManager.Core.Interfaces
{
    public interface IUserSettingsService<T> where T : new()
    {
        T Settings { get; set; }

        void Save();
    }
}