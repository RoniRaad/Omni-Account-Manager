using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Services;
using AccountManager.Infrastructure.Services;
using Moq;

namespace AccountManager.Tests.Core.Services
{
    public sealed class AppStateTests
    {
        private readonly Mock<IAccountService> _accountService;
        private readonly Mock<IIpcService> _ipcService;
        private readonly IAppState _sut;
        public AppStateTests()
        {
            _accountService = new();
            _ipcService = new();
            _sut = new AppState(_accountService.Object, _ipcService.Object);
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
            var updatedRanking = "UpdatedRanking";
            var firstAccountGuid = Guid.NewGuid();
            var secondAccountGuid = Guid.NewGuid();
            var thirdAccountGuid = Guid.NewGuid();

            var minAccounts = new List<Account>()
            {
                new Account() { Id = firstAccountGuid, Name = "TestId1"},
                new Account() { Id = secondAccountGuid, Name = "TestId2"},
                new Account() { Id = thirdAccountGuid, Name = "TestId3"}
            };
            var fullAccounts = new List<Account>()
            {
                new Account() { Id = firstAccountGuid, Name = "TestId1", PlatformId = updatedPlatformId },
                new Account() { Id = secondAccountGuid, Name = "TestId2", PlatformId = updatedPlatformId },
                new Account() { Id = thirdAccountGuid, Name = "TestId3", PlatformId = updatedPlatformId }
            };

            _sut.Accounts = minAccounts;

            _accountService.Setup((x) => x.GetAllAccountsMinAsync()).ReturnsAsync(minAccounts);
            _accountService.Setup((x) => x.GetAllAccountsAsync()).ReturnsAsync(fullAccounts);
            _accountService.Setup((x) => x.WriteAllAccountsAsync(It.Is<List<Account>>((accounts) => accounts.TrueForAll((account) => account.PlatformId == updatedPlatformId )))).Verifiable();

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
                new Account() {Name = "TestId1"},
                new Account() {Name = "TestId2"},
                new Account() {Name = "TestId3"}
            };

            var fullAccounts = new List<Account>()
            {
                new Account() {Name = "TestId1", PlatformId = "UpdatedPlatformId"},
                new Account() {Name = "TestId2", PlatformId = "UpdatedPlatformId"},
                new Account() {Name = "TestId3", PlatformId = "UpdatedPlatformId"}
            };

            _sut.Accounts = minAccounts;

            _accountService.Setup((x) => x.GetAllAccountsMinAsync()).ReturnsAsync(minAccounts);
            _accountService.Setup((x) => x.GetAllAccountsAsync()).ReturnsAsync(fullAccounts);
            _accountService.Setup((x) => x.WriteAllAccountsAsync(It.Is<List<Account>>((accounts) => accounts.TrueForAll((account) => account.PlatformId == "UpdatedPlatformId" ))));

            // Act
            await _sut.UpdateAccounts();

            // Assert
            Assert.True(_sut.IsInitialized);
        }
    }
}
