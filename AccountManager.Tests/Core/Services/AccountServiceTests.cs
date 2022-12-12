using AccountManager.Core.Enums;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Services;
using AccountManager.Infrastructure.Repositories;
using AutoFixture;
using Moq;

namespace AccountManager.Tests.Core.Services
{
    public sealed class AccountServiceTests
    {
        private readonly Mock<IAccountEncryptedRepository> _accountRepo;
        private readonly Mock<IAuthService> _authService;
        private readonly Mock<IGenericFactory<AccountType, IPlatformService>> _platformServiceFactory;
        private readonly Mock<IPlatformService> _platformService;
        private readonly AccountService _sut;

        public AccountServiceTests()
        {
            _accountRepo = new Mock<IAccountEncryptedRepository>();
            _authService = new Mock<IAuthService>();
            _platformServiceFactory = new Mock<IGenericFactory<AccountType, IPlatformService>>();
            _sut = new AccountService(_platformServiceFactory.Object, _accountRepo.Object, _authService.Object);
            _platformService = new Mock<IPlatformService>();
        }

        [Fact]
        public void RemoveAccount_RemovesAccount_WhenAccountExists()
        {
            // Arrange
            var fixture = new Fixture();
            var accounts = fixture.Create<List<Account>>();
            var testAccount = accounts.First();

            //_iOService.Setup((x) => x.ReadData<List<Account>>(It.IsAny<string>())).Returns(accounts);
            //_iOService.Setup((x) => x.WriteData<List<Account>>(It.IsAny<List<Account>>(), It.IsAny<string>()));

            // Act
            _sut.DeleteAccountAsync(testAccount);

            // Assert
            Assert.DoesNotContain(testAccount, accounts);
        }

        [Fact]
        public async Task GetAllAccountsMin_GetsAllAccounts_WhenAccountsExist()
        {
            // Arrange
            var fixture = new Fixture();
            var accounts = fixture.Create<List<Account>>();

           // _iOService.Setup((x) => x.ReadData<List<Account>>(It.IsAny<string>())).Returns(accounts);

            // Act
            var value = await _sut.GetAllAccountsAsync();

            // Assert
            Assert.Equal(accounts, value);
        }

        [Fact]
        public async Task GetAllAccounts_ReturnsAllAccount_WhenAccountsExist()
        {
            // Arrange
            var fixture = new Fixture();
            var accounts = fixture.Create<List<Account>>();

            //_iOService.Setup((x) => x.ReadData<List<Account>>(It.IsAny<string>())).Returns(accounts);
            _platformServiceFactory.Setup((x) => x.CreateImplementation(It.IsAny<AccountType>())).Returns(_platformService.Object);
            _platformService.Setup((x) => x.TryFetchId(It.IsAny<Account>())).ReturnsAsync((true, "UpdatedId"));
            _platformService.Setup((x) => x.TryFetchRank(It.IsAny<Account>())).ReturnsAsync((true, new Rank() { Tier = "TestTier" }));

            // Act
            var value = await _sut.GetAllAccountsAsync();

            // Assert
            Assert.Equal(value, accounts);
        }


        [Fact]
        public async Task GetAllAccounts_UpdatesPlatformId_WhenPlatformIdIsEmptyOrNull()
        {
            // Arrange
            var fixture = new Fixture();
            var accounts = fixture.Create<List<Account>>();
            accounts.ForEach((account) => account.PlatformId = "");

            //_iOService.Setup((x) => x.ReadData<List<Account>>(It.IsAny<string>())).Returns(accounts);
            _platformServiceFactory.Setup((x) => x.CreateImplementation(It.IsAny<AccountType>())).Returns(_platformService.Object);
            _platformService.Setup((x) => x.TryFetchId(It.IsAny<Account>())).ReturnsAsync((true, "UpdatedId"));
            _platformService.Setup((x) => x.TryFetchRank(It.IsAny<Account>())).ReturnsAsync((true, new Rank() { Tier = "TestTier" }));

            // Act
            var value = await _sut.GetAllAccountsAsync();

            // Assert
            Assert.True(value.TrueForAll((x) => x.PlatformId == "UpdatedId"));
        }

        [Fact]
        public async Task GetAllAccounts_DoesNotUpdatesPlatformId_WhenItAlreadyExists()
        {
            // Arrange
            var fixture = new Fixture();
            var accounts = fixture.Create<List<Account>>();
            accounts.ForEach((account) => account.PlatformId = "InitialId");

            //_iOService.Setup((x) => x.ReadData<List<Account>>(It.IsAny<string>())).Returns(accounts);
            _platformServiceFactory.Setup((x) => x.CreateImplementation(It.IsAny<AccountType>())).Returns(_platformService.Object);
            _platformService.Setup((x) => x.TryFetchId(It.IsAny<Account>())).ReturnsAsync((true, "UpdatedId"));
            _platformService.Setup((x) => x.TryFetchRank(It.IsAny<Account>())).ReturnsAsync((true, new Rank() { Tier = "TestTier" }));

            // Act
            var value = await _sut.GetAllAccountsAsync();

            // Assert
            Assert.True(value.TrueForAll((x) => x.PlatformId == "InitialId" ));
        }
    }
}
