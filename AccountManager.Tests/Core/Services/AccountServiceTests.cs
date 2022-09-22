using AccountManager.Core.Enums;
using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Services;
using AutoFixture;
using Moq;

namespace AccountManager.Tests.Core.Services
{
    public sealed class AccountServiceTests
    {
        private readonly Mock<IIOService> _iOService;
        private readonly Mock<IAuthService> _authService;
        private readonly Mock<IGenericFactory<AccountType, IPlatformService>> _platformServiceFactory;
        private readonly Mock<IPlatformService> _platformService;
        private readonly AccountService _sut;

        public AccountServiceTests()
        {
            _iOService = new Mock<IIOService>();
            _authService = new Mock<IAuthService>();
            _platformServiceFactory = new Mock<IGenericFactory<AccountType, IPlatformService>>();
            _sut = new AccountService(_iOService.Object, _authService.Object, _platformServiceFactory.Object);
            _platformService = new Mock<IPlatformService>();
        }

        [Fact]
        public void RemoveAccount_RemovesAccount_WhenAccountExists()
        {
            // Arrange
            var fixture = new Fixture();
            var accounts = fixture.Create<List<Account>>();
            var testAccount = accounts.First();

            _iOService.Setup((x) => x.ReadData<List<Account>>(It.IsAny<string>())).Returns(accounts);
            _iOService.Setup((x) => x.UpdateData<List<Account>>(It.IsAny<List<Account>>(), It.IsAny<string>()));

            // Act
            _sut.RemoveAccount(testAccount);

            // Assert
            Assert.DoesNotContain(testAccount, accounts);
        }

        [Fact]
        public void GetAllAccountsMin_GetsAllAccounts_WhenAccountsExist()
        {
            // Arrange
            var fixture = new Fixture();
            var accounts = fixture.Create<List<Account>>();

            _iOService.Setup((x) => x.ReadData<List<Account>>(It.IsAny<string>())).Returns(accounts);

            // Act
            var value = _sut.GetAllAccountsMin();

            // Assert
            Assert.Equal(accounts, value);
        }

        [Fact]
        public async Task GetAllAccounts_ReturnsAllAccount_WhenAccountsExist()
        {
            // Arrange
            var fixture = new Fixture();
            var accounts = fixture.Create<List<Account>>();

            _iOService.Setup((x) => x.ReadData<List<Account>>(It.IsAny<string>())).Returns(accounts);
            _platformServiceFactory.Setup((x) => x.CreateImplementation(It.IsAny<AccountType>())).Returns(_platformService.Object);
            _platformService.Setup((x) => x.TryFetchId(It.IsAny<Account>())).ReturnsAsync((true, "UpdatedId"));
            _platformService.Setup((x) => x.TryFetchRank(It.IsAny<Account>())).ReturnsAsync((true, new Rank() { Tier = "TestTier" }));

            // Act
            var value = await _sut.GetAllAccounts();

            // Assert
            Assert.Equal(value, accounts);
        }

        [Fact]
        public async Task GetAllAccounts_UpdatesRank_WhenNewerDataIsReturned()
        {
            // Arrange
            var fixture = new Fixture();
            var accounts = fixture.Create<List<Account>>();
            accounts.ForEach((account) => account.PlatformId = "");

            _iOService.Setup((x) => x.ReadData<List<Account>>(It.IsAny<string>())).Returns(accounts);
            _platformServiceFactory.Setup((x) => x.CreateImplementation(It.IsAny<AccountType>())).Returns(_platformService.Object);
            _platformService.Setup((x) => x.TryFetchId(It.IsAny<Account>())).ReturnsAsync((true, "UpdatedId"));
            _platformService.Setup((x) => x.TryFetchRank(It.IsAny<Account>())).ReturnsAsync((true, new Rank() { Tier = "TestTier"}));

            // Act
            var value = await _sut.GetAllAccounts();

            // Assert
            Assert.True(value.TrueForAll((x) => x?.Rank?.Tier == "TestTier"));
        }

        [Fact]
        public async Task GetAllAccounts_UpdatesPlatformId_WhenPlatformIdIsEmptyOrNull()
        {
            // Arrange
            var fixture = new Fixture();
            var accounts = fixture.Create<List<Account>>();
            accounts.ForEach((account) => account.PlatformId = "");

            _iOService.Setup((x) => x.ReadData<List<Account>>(It.IsAny<string>())).Returns(accounts);
            _platformServiceFactory.Setup((x) => x.CreateImplementation(It.IsAny<AccountType>())).Returns(_platformService.Object);
            _platformService.Setup((x) => x.TryFetchId(It.IsAny<Account>())).ReturnsAsync((true, "UpdatedId"));
            _platformService.Setup((x) => x.TryFetchRank(It.IsAny<Account>())).ReturnsAsync((true, new Rank() { Tier = "TestTier" }));

            // Act
            var value = await _sut.GetAllAccounts();

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

            _iOService.Setup((x) => x.ReadData<List<Account>>(It.IsAny<string>())).Returns(accounts);
            _platformServiceFactory.Setup((x) => x.CreateImplementation(It.IsAny<AccountType>())).Returns(_platformService.Object);
            _platformService.Setup((x) => x.TryFetchId(It.IsAny<Account>())).ReturnsAsync((true, "UpdatedId"));
            _platformService.Setup((x) => x.TryFetchRank(It.IsAny<Account>())).ReturnsAsync((true, new Rank() { Tier = "TestTier" }));

            // Act
            var value = await _sut.GetAllAccounts();

            // Assert
            Assert.True(value.TrueForAll((x) => x.PlatformId == "InitialId" ));
        }
    }
}
