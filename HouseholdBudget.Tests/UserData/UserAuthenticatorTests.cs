using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using FluentAssertions;
using HouseholdBudget.Core.Data;
using HouseholdBudget.Core.Models;
using HouseholdBudget.Core.UserData;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace HouseholdBudget.Tests.UserData
{
    public class UserAuthenticatorTests
    {
        private readonly Mock<IBudgetRepository> _repoMock = new();
        private readonly Mock<IPasswordHasher<User>> _hasherMock = new();
        private readonly UserAuthenticator _authenticator;

        public UserAuthenticatorTests()
        {
            _authenticator = new UserAuthenticator(_repoMock.Object, _hasherMock.Object);
        }

        [Fact]
        public async Task AuthenticateAsync_WithCorrectCredentials_ShouldReturnUser()
        {
            // Arrange
            var user = User.Create("John", "john@example.com", "HASH", "USD");
            _repoMock.Setup(r => r.GetUserByEmailAsync("john@example.com"))
                     .ReturnsAsync(user);

            _hasherMock.Setup(h => h.VerifyHashedPassword(user, user.PasswordHash, "Password123"))
                       .Returns(PasswordVerificationResult.Success);

            // Act
            var result = await _authenticator.AuthenticateAsync("john@example.com", "Password123");

            // Assert
            result.Should().NotBeNull();
            result!.Email.Should().Be("john@example.com");
        }

        [Fact]
        public async Task AuthenticateAsync_WithWrongPassword_ShouldReturnNull()
        {
            var user = User.Create("John", "john@example.com", "HASH", "USD");
            _repoMock.Setup(r => r.GetUserByEmailAsync("john@example.com"))
                     .ReturnsAsync(user);

            _hasherMock.Setup(h => h.VerifyHashedPassword(user, user.PasswordHash, "WrongPass"))
                       .Returns(PasswordVerificationResult.Failed);

            var result = await _authenticator.AuthenticateAsync("john@example.com", "WrongPass");

            result.Should().BeNull();
        }

        [Fact]
        public async Task AuthenticateAsync_WithInvalidEmail_ShouldThrow()
        {
            Func<Task> act = async () => await _authenticator.AuthenticateAsync("bademail", "pass");
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*Email*");
        }

        [Fact]
        public async Task AuthenticateAsync_WithMissingPassword_ShouldThrow()
        {
            Func<Task> act = async () => await _authenticator.AuthenticateAsync("test@example.com", "");
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*Password is required*");
        }

        [Fact]
        public async Task RegisterAsync_WithValidData_ShouldReturnUser()
        {
            _repoMock.Setup(r => r.GetUserByEmailAsync(It.IsAny<string>())).ReturnsAsync((User)null!);
            _repoMock.Setup(r => r.AddUserAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
            _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
            _hasherMock.Setup(h => h.HashPassword(It.IsAny<User>(), "StrongPass1!"))
                       .Returns("HASH");

            var result = await _authenticator.RegisterAsync("Jane", "jane@example.com", "StrongPass1!", "USD");

            result.Should().NotBeNull();
            result.Email.Should().Be("jane@example.com");
        }

        [Fact]
        public async Task RegisterAsync_WithWeakPassword_ShouldThrowValidationException()
        {
            Func<Task> act = async () => await _authenticator.RegisterAsync("Jane", "jane@example.com", "123", "USD");

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*Password must be at least*");
        }

        [Fact]
        public async Task RegisterAsync_WhenEmailAlreadyExists_ShouldThrowValidationException()
        {
            var existingUser = User.Create("Existing", "taken@example.com", "HASH", "USD");
            _repoMock.Setup(r => r.GetUserByEmailAsync("taken@example.com"))
                     .ReturnsAsync(existingUser);

            Func<Task> act = async () => await _authenticator.RegisterAsync("New", "taken@example.com", "StrongPass1!", "USD");

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*already exists*");
        }
    }
}
