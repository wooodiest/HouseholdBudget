using FluentAssertions;
using HouseholdBudget.Core.UserData;
using System;
using Xunit;

namespace HouseholdBudget.Tests.UserData
{
    public class UserContextTests
    {
        [Fact]
        public void SetUser_ShouldStoreAndExposeUser()
        {
            // Arrange
            var context = new UserContext();
            var user = User.Create("Jane", "jane@example.com", "HASH", "USD");

            // Act
            context.SetUser(user);

            // Assert
            context.CurrentUser.Should().Be(user);
        }

        [Fact]
        public void Clear_ShouldRemoveUser()
        {
            // Arrange
            var context = new UserContext();
            var user = User.Create("Jane", "jane@example.com", "HASH", "USD");
            context.SetUser(user);

            // Act
            context.Clear();

            // Assert
            context.CurrentUser.Should().BeNull();
        }

        [Fact]
        public void CurrentUser_ShouldBeNullByDefault()
        {
            // Arrange
            var context = new UserContext();

            // Assert
            context.CurrentUser.Should().BeNull();
        }
    }
}
