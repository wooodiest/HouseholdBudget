using System.Threading.Tasks;
using FluentAssertions;
using HouseholdBudget.Core.UserData;
using Moq;
using Xunit;

namespace HouseholdBudget.Tests.UserData
{
    public class UserSessionServiceTests
    {
        private readonly Mock<IUserAuthenticator> _authMock = new();
        private readonly Mock<IUserContext> _contextMock = new();
        private readonly UserSessionService _service;

        public UserSessionServiceTests()
        {
            _service = new UserSessionService(_authMock.Object, _contextMock.Object);
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ShouldSetUserAndReturnTrue()
        {
            // Arrange
            var user = User.Create("Anna", "anna@example.com", "HASH", "USD");
            _authMock.Setup(a => a.AuthenticateAsync("anna@example.com", "pass"))
                     .ReturnsAsync(user);

            // Act
            var result = await _service.LoginAsync("anna@example.com", "pass");

            // Assert
            result.Should().BeTrue();
            _contextMock.Verify(c => c.SetUser(user), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_WithInvalidCredentials_ShouldReturnFalseAndNotSetUser()
        {
            // Arrange
            _authMock.Setup(a => a.AuthenticateAsync("wrong@example.com", "fail"))
                     .ReturnsAsync((User)null!);

            // Act
            var result = await _service.LoginAsync("wrong@example.com", "fail");

            // Assert
            result.Should().BeFalse();
            _contextMock.Verify(c => c.SetUser(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public void Logout_ShouldClearUserContext()
        {
            // Act
            _service.Logout();

            // Assert
            _contextMock.Verify(c => c.Clear(), Times.Once);
        }

        [Fact]
        public void GetUser_ShouldReturnCurrentUserFromContext()
        {
            // Arrange
            var user = User.Create("Tom", "tom@example.com", "HASH", "USD");
            _contextMock.Setup(c => c.CurrentUser).Returns(user);

            // Act
            var result = _service.GetUser();

            // Assert
            result.Should().Be(user);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsAuthenticated_ShouldReflectContextUserPresence(bool isAuthenticated)
        {
            // Arrange
            var user = isAuthenticated ? User.Create("Kate", "kate@example.com", "HASH", "USD") : null;
            _contextMock.Setup(c => c.CurrentUser).Returns(user);

            // Act
            var result = _service.IsAuthenticated;

            // Assert
            result.Should().Be(isAuthenticated);
        }
    }
}
