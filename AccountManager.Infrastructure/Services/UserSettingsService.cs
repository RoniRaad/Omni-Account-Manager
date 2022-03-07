using AccountManager.Core.Interfaces;

namespace AccountManager.Infrastructure.Services
{
    public class UserSettingsService<T> : IUserSettingsService<T> where T : new()
    {
        public T Settings { get; set; }
        private readonly IIOService _iOService;
        public UserSettingsService(IIOService iOService)
        {
            _iOService = iOService;
            Settings = _iOService.ReadData<T>();
        }

        public void Save() => _iOService.UpdateData(Settings);
    }
}
