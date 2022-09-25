﻿using AccountManager.Core.Interfaces;
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
                Guid = Guid.NewGuid(),
                Id = "TestId",
                Password = "TestPassword",
                PlatformId = "TestPlatformId",
                Rank = new Rank { HexColor = "TestHex", Ranking = "TestRanking", Tier = "TestTier" },
                Username = "TestUsername"
            };
            _sut.Accounts = new() { account };

            _accountService.Setup((x) => x.LoginAsync(account)).Verifiable();

            // Act
            await _sut.IpcLogin(new() { Guid = account.Guid });

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
                new Account() { Guid = firstAccountGuid, Id = "TestId1"},
                new Account() { Guid = secondAccountGuid, Id = "TestId2"},
                new Account() { Guid = thirdAccountGuid, Id = "TestId3"}
            };
            var fullAccounts = new List<Account>()
            {
                new Account() { Guid = firstAccountGuid, Id = "TestId1", PlatformId = updatedPlatformId, Rank = new(){ Ranking = updatedRanking }},
                new Account() { Guid = secondAccountGuid, Id = "TestId2", PlatformId = updatedPlatformId, Rank = new(){ Ranking = updatedRanking }},
                new Account() { Guid = thirdAccountGuid, Id = "TestId3", PlatformId = updatedPlatformId, Rank = new(){ Ranking = updatedRanking }}
            };

            _sut.Accounts = minAccounts;

            _accountService.Setup((x) => x.GetAllAccountsMinAsync()).ReturnsAsync(minAccounts);
            _accountService.Setup((x) => x.GetAllAccountsAsync()).ReturnsAsync(fullAccounts);
            _accountService.Setup((x) => x.WriteAllAccountsAsync(It.Is<List<Account>>((accounts) => accounts.TrueForAll((account) => account.PlatformId == updatedPlatformId && account.Rank.Ranking == updatedRanking)))).Verifiable();

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
                new Account() {Id = "TestId1"},
                new Account() {Id = "TestId2"},
                new Account() {Id = "TestId3"}
            };

            var fullAccounts = new List<Account>()
            {
                new Account() {Id = "TestId1", PlatformId = "UpdatedPlatformId", Rank = new(){ Ranking = "UpdatedRanking" }},
                new Account() {Id = "TestId2", PlatformId = "UpdatedPlatformId", Rank = new(){ Ranking = "UpdatedRanking" }},
                new Account() {Id = "TestId3", PlatformId = "UpdatedPlatformId", Rank = new(){ Ranking = "UpdatedRanking" }}
            };

            _sut.Accounts = minAccounts;

            _accountService.Setup((x) => x.GetAllAccountsMinAsync()).ReturnsAsync(minAccounts);
            _accountService.Setup((x) => x.GetAllAccountsAsync()).ReturnsAsync(fullAccounts);
            _accountService.Setup((x) => x.WriteAllAccountsAsync(It.Is<List<Account>>((accounts) => accounts.TrueForAll((account) => account.PlatformId == "UpdatedPlatformId" && account.Rank.Ranking == "UpdatedRanking"))));

            // Act
            await _sut.UpdateAccounts();

            // Assert
            Assert.True(_sut.IsInitialized);
        }
    }
}
