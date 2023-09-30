using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Models.UserSettings;
using AccountManager.Core.Services;
using AccountManager.Infrastructure.Services;
using Moq;

namespace AccountManager.Tests.Core.Services
{
    public sealed class AppStateTests
    {
        private readonly Mock<IAccountService> _accountService;
        private readonly Mock<IIpcService> _ipcService;
        private readonly Mock<IUserSettingsService<Dictionary<Guid, AccountListItemSettings>>> _accountListItemSettings;
        private readonly IAppState _sut;
        public AppStateTests()
        {
            _accountService = new();
            _ipcService = new();
            _accountListItemSettings = new();
            _sut = new AppState(_accountService.Object, _ipcService.Object, _accountListItemSettings.Object);
        }

        [Fact]
        public async Task AppState_ShouldAttemptLogin_WhenValidIpcRequestRecieved()
        {
            // Arrange
            var account = new Account()
            {
                AccountType = AccountManager.Core.Enums.AccountType.League,
                Id = Guid.NewGuid(),
                Name = "TestId",
                Password = "TestPassword",
                PlatformId = "TestPlatformId",
                Username = "TestUsername"
            };
            _sut.Accounts = new() { account };

            _accountService.Setup((x) => x.LoginAsync(account)).Verifiable();

            // Act
            await _sut.IpcLogin(new() { Guid = account.Id });

            // Assert
            Mock.Verify(_accountService);
        }

        [Fact]
        public async Task UpdateAccount_AttemptsToUpdatePlatformIdAndRank_WhenPlatformIdAndRankHaveChanged()
        {
            // Arrange
            var updatedPlatformId = "UpdatedPlatformId";
            var firstAccountGuid = Guid.NewGuid();
            var secondAccountGuid = Guid.NewGuid();
            var thirdAccountGuid = Guid.NewGuid();

            var minAccounts = new List<Account>()
            {
                new Account() { Id = firstAccountGuid, Name = "TestId1", Password = "", Username = ""},
                new Account() { Id = secondAccountGuid, Name = "TestId2", Password = "", Username = ""},
                new Account() {Id = thirdAccountGuid, Name = "TestId3", Password = "", Username = ""}
            };
            var fullAccounts = new List<Account>()
            {
                new Account() { Id = firstAccountGuid, Name = "TestId1", PlatformId = updatedPlatformId, Password = "", Username = "" },
                new Account() { Id = secondAccountGuid, Name = "TestId2", PlatformId = updatedPlatformId, Password = "", Username = "" },
                new Account() { Id = thirdAccountGuid, Name = "TestId3", PlatformId = updatedPlatformId, Password = "", Username = "" }
            };

            _sut.Accounts = minAccounts;

            _accountService.Setup((x) => x.GetAllAccountsAsync()).ReturnsAsync(minAccounts);
            _accountService.Setup((x) => x.GetAllAccountsAsync()).ReturnsAsync(fullAccounts);

            // Act
            await _sut.UpdateAccounts();

            // Assert
            Mock.Verify(_accountService);
        }

        [Fact]
        public async Task AppState_IsInitializedProperty_SetToTrueAfterUpdatingAccounts()
        {
            // Arrange
            var minAccounts = new List<Account>()
            {
                new Account() {Name = "TestId1", Username = "", Password = ""},
                new Account() {Name = "TestId2", Username = "", Password = ""},
                new Account() {Name = "TestId3", Username = "", Password = ""}
            };

            var fullAccounts = new List<Account>()
            {
                new Account() {Name = "TestId1", PlatformId = "UpdatedPlatformId", Username = "", Password = ""},
                new Account() {Name = "TestId2", PlatformId = "UpdatedPlatformId", Username = "", Password = ""},
                new Account() {Name = "TestId3", PlatformId = "UpdatedPlatformId", Username = "", Password = ""}
            };

            _sut.Accounts = minAccounts;

            _accountService.Setup((x) => x.GetAllAccountsAsync()).ReturnsAsync(minAccounts);
            _accountService.Setup((x) => x.GetAllAccountsAsync()).ReturnsAsync(fullAccounts);

            // Act
            await _sut.UpdateAccounts();

            // Assert
            Assert.True(_sut.IsInitialized);
        }
    }
}
