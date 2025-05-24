using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using HouseholdBudget.Core.UserData;

namespace HouseholdBudget.Tests.UserData
{
    public class UserTests
    {
        [Fact]
        public void Create_WithValidInput_ShouldReturnUser()
        {
            // Arrange
            var name = "John Doe";
            var email = "john@example.com";
            var passwordHash = "hashed_password_example";
            var currencyCode = "USD";

            // Act
            var user = User.Create(name, email, passwordHash, currencyCode);

            // Assert
            user.Name.Should().Be(name);
            user.Email.Should().Be(email.ToLowerInvariant());
            user.PasswordHash.Should().Be(passwordHash);
            user.DefaultCurrencyCode.Should().Be(currencyCode);
            user.Id.Should().NotBeEmpty();
            user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_WithInvalidName_ShouldThrowValidationException(string invalidName)
        {
            // Act
            Action act = () => User.Create(invalidName, "user@example.com", "hashed", "USD");

            // Assert
            act.Should().Throw<ValidationException>()
                .WithMessage("*Name is required*");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("bademail")]
        public void Create_WithInvalidEmail_ShouldThrowValidationException(string invalidEmail)
        {
            // Act
            Action act = () => User.Create("John", invalidEmail, "hashed", "USD");

            // Assert
            act.Should().Throw<ValidationException>()
                .WithMessage("*Email*");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_WithInvalidPasswordHash_ShouldThrowValidationException(string invalidHash)
        {
            // Act
            Action act = () => User.Create("John", "john@example.com", invalidHash, "USD");

            // Assert
            act.Should().Throw<ValidationException>()
                .WithMessage("*Password hash is required*");
        }

        [Fact]
        public void ValidatePasswordHash_ShouldReturnError_WhenMissing()
        {
            var result = User.ValidatePasswordHash("");
            result.Should().ContainSingle().Which.Should().Contain("Password hash is required");
        }

        [Fact]
        public void ValidateEmail_ShouldReturnError_WhenInvalid()
        {
            var result = User.ValidateEmail("bademail");
            result.Should().Contain(e => e.Contains("Email"));
        }

        [Fact]
        public void Validate_ShouldReturnAllErrors_WhenMultipleInvalidInputs()
        {
            var result = User.Validate("", "bademail", "", "XX");

            result.Should().Contain(e => e.Contains("Name is required"));
            result.Should().Contain(e => e.Contains("Email format is invalid"));
            result.Should().Contain(e => e.Contains("Password hash is required"));
        }
    }
}
