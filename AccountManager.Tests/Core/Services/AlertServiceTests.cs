using AccountManager.Core.Services;

namespace AccountManager.Tests.Core.Services
{
    public sealed class AlertServiceTests
    {
        private readonly AlertService _sut;

        public AlertServiceTests()
        {
            _sut = new AlertService();
        }

        [Fact]
        public void AlertService_AddsInfoMessage_WhenMessageIsNotEmpty()
        {
            // Act
            var testMessage = "testMessage";
            _sut.AddInfoAlert(testMessage);

            // Assert
            Assert.NotNull(_sut.GetInfoAlerts()
                .FirstOrDefault((alert) => alert.DisplayMessage == testMessage));
        }

        [Fact]
        public void AlertService_AddsErrorMessage_WhenMessageIsNotEmpty()
        {
            // Act
            var testMessage = "testMessage";
            _sut.AddErrorAlert(testMessage);

            // Assert
            Assert.NotNull(_sut.GetErrorAlerts()
              .FirstOrDefault((alert) => alert.DisplayMessage == testMessage));
        }

        [Fact]
        public void AlertService_RemovesInfoMessage_WhenMessageIsInQueue()
        {
            // Act
            var testMessage = "testMessage";
            _sut.AddInfoAlert(testMessage);
            var message = _sut.GetInfoAlerts()
                .First((alert) => alert.DisplayMessage == testMessage);
            _sut.RemoveInfoMessage(message);

            // Assert
            Assert.DoesNotContain(message, _sut.GetInfoAlerts());
        }

        [Fact]
        public void AlertService_RemovesErrorMessage_WhenMessageIsInQueue()
        {
            // Act
            var testMessage = "testMessage";
            _sut.AddErrorAlert(testMessage);
            var message = _sut.GetErrorAlerts()
                .First((alert) => alert.DisplayMessage == testMessage);
            _sut.RemoveErrorMessage(message);

            // Assert
            Assert.DoesNotContain(message, _sut.GetErrorAlerts());
        }

        [Fact]
        public void AlertService_DoesNotAddInfoMessage_WhenMessageIsEmpty()
        {
            // Act
            var testMessage = "";
            _sut.AddInfoAlert(testMessage);

            // Assert
            Assert.Null(_sut.GetInfoAlerts()
                .FirstOrDefault((alert) => alert.DisplayMessage == testMessage));
        }

        [Fact]
        public void AlertService_DoesNotAddErrorAlert_WhenMessageIsEmpty()
        {
            // Act
            var testMessage = "";
            _sut.AddErrorAlert(testMessage);

            // Assert
            Assert.Null(_sut.GetErrorAlerts()
                .FirstOrDefault((alert) => alert.DisplayMessage == testMessage));
        }
    }
}
