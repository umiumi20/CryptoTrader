using Xunit;
using CryptoTrader.Core.Common.Constants;

namespace CryptoTrader.Tests.Common
{
    public class SystemConstantsTests
    {
        [Fact]
        public void SystemTime_ShouldBeValid()
        {
            // Arrange & Act
            var systemTime = SystemConstants.SYSTEM_TIME;

            // Assert
            Assert.NotEqual(default(DateTime), systemTime);
        }

        [Fact]
        public void UserLogin_ShouldBeValid()
        {
            // Arrange & Act
            var userLogin = SystemConstants.USER_LOGIN;

            // Assert
            Assert.Equal("umiumi20", userLogin);
        }
    }
}