using AccountManager.Core.Interfaces;
using AccountManager.Core.Models;
using AccountManager.Core.Services;
using AccountManager.Core.Static;
using Microsoft.Extensions.Caching.Distributed;
using Moq;

namespace AccountManager.Tests.Core.Services
{
    public sealed class AuthServiceTests
    {
        private readonly Mock<IGeneralFileSystemService> _iOService;
        private readonly Mock<IAlertService> _alertService;
        private readonly Mock<IDistributedCache> _persistantCache;
        private readonly Mock<IAccountEncryptedRepository> _accountRepo;
        private readonly SqliteAuthService _sut;

        public AuthServiceTests()
        {
            _iOService = new Mock<IGeneralFileSystemService>();
            _alertService = new Mock<IAlertService>();
            _persistantCache = new Mock<IDistributedCache>();
            _accountRepo = new Mock<IAccountEncryptedRepository>();
            _sut = new(_accountRepo.Object, _alertService.Object, _persistantCache.Object, _iOService.Object);
        }

        [Fact]
        public async Task Login_TriesToDecryptData_WithHashedSuppliedPassword()
        {
            // Arrange
            var testPassword = "password";
            _iOService.Setup((x) => x.TryReadEncryptedData(It.Is<string>((val) => val == StringEncryption.Hash(testPassword)))).Returns(true).Verifiable();

            // Act
            await _sut.LoginAsync(testPassword);

            // Assert
            Mock.Verify(_iOService);
        }

        [Fact]
        public async Task Login_LogsUserIn_WhenCorrectPasswordIsGiven()
        {
            // Arrange
            var testPassword = "password";
            _iOService.Setup((x) => x.TryReadEncryptedData(It.Is<string>((val) => val == StringEncryption.Hash(testPassword)))).Returns(true);

            // Act
            await _sut.LoginAsync(testPassword);

            // Assert
            Assert.True(_sut.LoggedIn);
        }

        [Fact]
        public void Register_UsesHashedPasswordToSaveData()
        {
            // Arrange
            var testPassword = "password";
            _iOService.Setup((x) => x.WriteData(It.IsAny<List<object>>(), It.Is<string>((val) => val == StringEncryption.Hash(testPassword)))).Verifiable();

            // Act
            _sut.Register(testPassword);

            // Assert
            Mock.Verify(_iOService);
        }

        [Fact]
        public void Register_LoggesUserIn()
        {
            // Arrange
            var testPassword = "password";
            _iOService.Setup((x) => x.WriteData(It.IsAny<List<object>>(), It.Is<string>((val) => val == StringEncryption.Hash(testPassword))));

            // Act
            _sut.Register(testPassword);

            // Assert
            Assert.True(_sut.LoggedIn);
        }

        [Fact]
        public void ChangePassword_RewritesDataWithNewPassword()
        {
            // Arrange
            var testInitialPassword = "password";
            var testUpdatedPassword = "updatePassword";
            _iOService.Setup((x) => x.ReadData<List<Account>>(It.Is<string>((val) => val == StringEncryption.Hash(testInitialPassword)))).Verifiable();
            _iOService.Setup((x) => x.WriteData(It.IsAny<List<Account>>(), It.Is<string>((val) => val == StringEncryption.Hash(testUpdatedPassword)))).Verifiable();
            _iOService.Setup((x) => x.TryReadEncryptedData(It.Is<string>((val) => val == testInitialPassword))).Returns(true);

            // Act
            _sut.ChangePassword(testInitialPassword, testUpdatedPassword);

            // Assert
            Mock.Verify(_iOService);
        }

        [Fact]
        public void ChangePassword_SetsNewPasswordHash()
        {
            // Arrange
            var testInitialPassword = "password";
            var testUpdatedPassword = "updatePassword";
            _iOService.Setup((x) => x.WriteData(It.IsAny<List<Account>>(), It.Is<string>((val) => val == StringEncryption.Hash(testUpdatedPassword)))).Verifiable();
            _iOService.Setup((x) => x.ReadData<List<Account>>(It.Is<string>((val) => val == StringEncryption.Hash(testInitialPassword)))).Verifiable();
            _iOService.Setup((x) => x.TryReadEncryptedData(It.Is<string>((val) => val == testInitialPassword))).Returns(true);

            // Act
            _sut.ChangePassword(testInitialPassword, testUpdatedPassword);

            // Assert
            Assert.Equal(StringEncryption.Hash(testUpdatedPassword), _sut.PasswordHash);
        }

        [Fact]
        public void ChangePassword_DoesNotChangePassword_WhenOldPasswordIsEmpty()
        {
            // Arrange
            var testInitialPassword = "";
            var testUpdatedPassword = "updatePassword";
            _iOService.Setup((x) => x.WriteData(It.IsAny<List<Account>>(), It.Is<string>((val) => val == StringEncryption.Hash(testUpdatedPassword)))).Throws(new Exception("Should not be called. initial password was empty"));
            _iOService.Setup((x) => x.ReadData<List<Account>>(It.Is<string>((val) => val == StringEncryption.Hash(testInitialPassword)))).Throws(new Exception("Should not be called. initial password was empty"));
            _iOService.Setup((x) => x.TryReadEncryptedData(It.Is<string>((val) => val == testInitialPassword))).Returns(true);

            // Act
            _sut.ChangePassword(testInitialPassword, testUpdatedPassword);

            // Assert
            Mock.Verify(_iOService);
        }

        [Fact]
        public void ChangePassword_DoesNotChangePassword_WhenOldPasswordIsNull()
        {
            // Arrange
            string? testInitialPassword = "";
            var testUpdatedPassword = "updatePassword";
            _iOService.Setup((x) => x.WriteData(It.IsAny<List<Account>>(), It.Is<string>((val) => val == StringEncryption.Hash(testUpdatedPassword)))).Throws(new Exception("Should not be called. initial password was empty"));
            _iOService.Setup((x) => x.ReadData<List<Account>>(It.Is<string>((val) => val == null))).Throws(new Exception("Should not be called. initial password was empty"));
            _iOService.Setup((x) => x.TryReadEncryptedData(It.Is<string>((val) => val == testInitialPassword))).Returns(true);

            // Act
            _sut.ChangePassword(testInitialPassword, testUpdatedPassword);

            // Assert
            Mock.Verify(_iOService);
        }

        [Fact]
        public void ChangePassword_DoesNotChangePassword_WhenNewPasswordIsNull()
        {
            // Arrange
            var testInitialPassword = "password";
            string testUpdatedPassword = "";
            _iOService.Setup((x) => x.WriteData(It.IsAny<List<Account>>(), It.Is<string>((val) => val == null))).Throws(new Exception("Should not be called. new password was empty"));
            _iOService.Setup((x) => x.ReadData<List<Account>>(It.Is<string>((val) => val == StringEncryption.Hash(testInitialPassword)))).Throws(new Exception("Should not be called. new password was empty"));
            _iOService.Setup((x) => x.TryReadEncryptedData(It.Is<string>((val) => val == testInitialPassword))).Returns(true);

            // Act
            _sut.ChangePassword(testInitialPassword, testUpdatedPassword);

            // Assert
            Mock.Verify(_iOService);
        }

        [Fact]
        public void ChangePassword_DoesNotChangePassword_WhenNewPasswordIsEmpty()
        {
            // Arrange
            var testInitialPassword = "password";
            var testUpdatedPassword = "";
            _iOService.Setup((x) => x.WriteData(It.IsAny<List<Account>>(), It.Is<string>((val) => val == StringEncryption.Hash(testUpdatedPassword)))).Throws(new Exception("Should not be called. new password was empty"));
            _iOService.Setup((x) => x.ReadData<List<Account>>(It.Is<string>((val) => val == StringEncryption.Hash(testInitialPassword)))).Throws(new Exception("Should not be called. new password was empty"));
            _iOService.Setup((x) => x.TryReadEncryptedData(It.Is<string>((val) => val == testInitialPassword))).Returns(true);

            // Act
            _sut.ChangePassword(testInitialPassword, testUpdatedPassword);

            // Assert
            Mock.Verify(_iOService);
        }
    }
}
