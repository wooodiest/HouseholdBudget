using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using HouseholdBudget.Core.Models;

namespace HouseholdBudget.Tests.Models
{
    public class CategoryTests
    {
        [Fact]
        public void Create_WithValidInput_ShouldReturnProperlyInitializedCategory()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var name = "Groceries";
            var type = CategoryType.Expense;

            // Act
            var category = Category.Create(userId, name, type);

            // Assert
            category.UserId.Should().Be(userId);
            category.Name.Should().Be(name);
            category.Type.Should().Be(type);
            category.Id.Should().NotBeEmpty();
            category.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            category.UpdatedAt.Should().BeNull();
        }

        [Fact]
        public void Create_WithNullName_ShouldThrowValidationException()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act
            Action act = () => Category.Create(userId, null!);

            // Assert
            act.Should()
                .Throw<ValidationException>()
                .WithMessage("*Category name is required*");
        }

        [Fact]
        public void Create_WithEmptyName_ShouldThrowValidationException()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act
            Action act = () => Category.Create(userId, "");

            // Assert
            act.Should()
                .Throw<ValidationException>()
                .WithMessage("*Category name is required*");
        }

        [Fact]
        public void Create_WithWhitespaceName_ShouldThrowValidationException()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act
            Action act = () => Category.Create(userId, "   ");

            // Assert
            act.Should()
                .Throw<ValidationException>()
                .WithMessage("*Category name is required*");
        }

        [Fact]
        public void Create_WithTooLongName_ShouldThrowValidationException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var tooLongName = new string('a', Category.MaxNameLength + 1);

            // Act
            Action act = () => Category.Create(userId, tooLongName);

            // Assert
            act.Should()
                .Throw<ValidationException>()
                .WithMessage($"*cannot exceed {Category.MaxNameLength}*");
        }

        [Fact]
        public void Rename_WithValidName_ShouldUpdateNameAndTimestamp()
        {
            // Arrange
            var category = Category.Create(Guid.NewGuid(), "Transport");
            var newName = "Public Transport";

            // Act
            category.Rename(newName);

            // Assert
            category.Name.Should().Be(newName);
            category.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Rename_WithNullName_ShouldThrowValidationException()
        {
            // Arrange
            var category = Category.Create(Guid.NewGuid(), "Health");

            // Act
            Action act = () => category.Rename(null!);

            // Assert
            act.Should()
                .Throw<ValidationException>()
                .WithMessage("*Category name is required*");
        }

        [Fact]
        public void Rename_WithEmptyName_ShouldThrowValidationException()
        {
            // Arrange
            var category = Category.Create(Guid.NewGuid(), "Health");

            // Act
            Action act = () => category.Rename("");

            // Assert
            act.Should()
                .Throw<ValidationException>()
                .WithMessage("*Category name is required*");
        }

        [Fact]
        public void Rename_WithWhitespaceName_ShouldThrowValidationException()
        {
            // Arrange
            var category = Category.Create(Guid.NewGuid(), "Health");

            // Act
            Action act = () => category.Rename("   ");

            // Assert
            act.Should()
                .Throw<ValidationException>()
                .WithMessage("*Category name is required*");
        }

        [Fact]
        public void Rename_WithTooLongName_ShouldThrowValidationException()
        {
            // Arrange
            var category = Category.Create(Guid.NewGuid(), "Bills");
            var tooLongName = new string('X', Category.MaxNameLength + 1);

            // Act
            Action act = () => category.Rename(tooLongName);

            // Assert
            act.Should()
                .Throw<ValidationException>()
                .WithMessage($"*cannot exceed {Category.MaxNameLength}*");
        }

        [Fact]
        public void ChangeType_WithDifferentType_ShouldUpdateTypeAndTimestamp()
        {
            // Arrange
            var category = Category.Create(Guid.NewGuid(), "Bonus", CategoryType.Income);

            // Act
            category.ChangeType(CategoryType.Expense);

            // Assert
            category.Type.Should().Be(CategoryType.Expense);
            category.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void ChangeType_WithSameType_ShouldNotChangeAnything()
        {
            // Arrange
            var category = Category.Create(Guid.NewGuid(), "Rent", CategoryType.Expense);

            // Act
            category.ChangeType(CategoryType.Expense);

            // Assert
            category.Type.Should().Be(CategoryType.Expense);
            category.UpdatedAt.Should().BeNull();
        }
    }
}
