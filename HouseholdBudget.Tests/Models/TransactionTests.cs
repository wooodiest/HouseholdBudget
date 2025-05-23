using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using HouseholdBudget.Core.Models;

namespace HouseholdBudget.Tests.Models
{
    public class TransactionTests
    {
        private static readonly Currency ValidCurrency = Currency.Create("USD", "$", "US Dollar");

        [Fact]
        public void Create_WithValidInput_ShouldReturnProperlyInitializedTransaction()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var amount = 50.00m;
            var tags = new[] { "food", "grocery" };
            var description = "Weekly shopping";
            var date = DateTime.UtcNow.AddDays(-1);

            // Act
            var transaction = Transaction.Create(userId, categoryId, amount, ValidCurrency, TransactionType.Expense, description, tags, date);

            // Assert
            transaction.UserId.Should().Be(userId);
            transaction.CategoryId.Should().Be(categoryId);
            transaction.Amount.Should().Be(amount);
            transaction.Currency.Should().Be(ValidCurrency);
            transaction.Type.Should().Be(TransactionType.Expense);
            transaction.Description.Should().Be(description);
            transaction.Tags.Should().BeEquivalentTo(tags);
            transaction.Date.Should().BeCloseTo(date, TimeSpan.FromSeconds(1));
            transaction.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Create_WithEmptyUserId_ShouldThrowValidationException()
        {
            // Act
            Action act = () => Transaction.Create(Guid.Empty, Guid.NewGuid(), 10.0m, ValidCurrency);

            // Assert
            act.Should().Throw<ValidationException>().WithMessage("*UserId is required*");
        }

        [Fact]
        public void Create_WithEmptyCategoryId_ShouldThrowValidationException()
        {
            // Act
            Action act = () => Transaction.Create(Guid.NewGuid(), Guid.Empty, 10.0m, ValidCurrency);

            // Assert
            act.Should().Throw<ValidationException>().WithMessage("*CategoryId is required*");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public void Create_WithInvalidAmount_ShouldThrowValidationException(decimal amount)
        {
            // Act
            Action act = () => Transaction.Create(Guid.NewGuid(), Guid.NewGuid(), amount, ValidCurrency);

            // Assert
            act.Should().Throw<ValidationException>().WithMessage("*Amount must be greater than zero*");
        }

        [Fact]
        public void Create_WithNullCurrency_ShouldThrowValidationException()
        {
            // Act
            Action act = () => Transaction.Create(Guid.NewGuid(), Guid.NewGuid(), 10.0m, null!);

            // Assert
            act.Should().Throw<ValidationException>().WithMessage("*Currency is required*");
        }

        [Fact]
        public void Create_WithTooLongDescription_ShouldThrowValidationException()
        {
            // Arrange
            var longDescription = new string('A', Transaction.MaxDescriptionLength + 1);

            // Act
            Action act = () => Transaction.Create(Guid.NewGuid(), Guid.NewGuid(), 10.0m, ValidCurrency, description: longDescription);

            // Assert
            act.Should().Throw<ValidationException>().WithMessage("*Description cannot exceed*");
        }

        [Fact]
        public void Create_WithDuplicateTags_ShouldThrowValidationException()
        {
            // Arrange
            var tags = new[] { "food", "FOOD" };

            // Act
            Action act = () => Transaction.Create(Guid.NewGuid(), Guid.NewGuid(), 10.0m, ValidCurrency, tags: tags);

            // Assert
            act.Should().Throw<ValidationException>().WithMessage("*Tags must be unique*");
        }

        [Fact]
        public void UpdateAmount_WithValidAmount_ShouldUpdateAndSetTimestamp()
        {
            // Arrange
            var transaction = Transaction.Create(Guid.NewGuid(), Guid.NewGuid(), 10.0m, ValidCurrency);

            // Act
            transaction.UpdateAmount(25.0m);

            // Assert
            transaction.Amount.Should().Be(25.0m);
            transaction.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void UpdateAmount_WithInvalidAmount_ShouldThrowValidationException()
        {
            // Arrange
            var transaction = Transaction.Create(Guid.NewGuid(), Guid.NewGuid(), 10.0m, ValidCurrency);

            // Act
            Action act = () => transaction.UpdateAmount(0);

            // Assert
            act.Should().Throw<ValidationException>().WithMessage("*greater than zero*");
        }

        [Fact]
        public void UpdateDescription_WithTooLongText_ShouldThrowValidationException()
        {
            // Arrange
            var transaction = Transaction.Create(Guid.NewGuid(), Guid.NewGuid(), 10.0m, ValidCurrency);
            var longDescription = new string('X', Transaction.MaxDescriptionLength + 1);

            // Act
            Action act = () => transaction.UpdateDescription(longDescription);

            // Assert
            act.Should().Throw<ValidationException>().WithMessage("*cannot exceed*");
        }

        [Fact]
        public void UpdateTags_WithEmptyOrDuplicateTags_ShouldThrowValidationException()
        {
            // Arrange
            var transaction = Transaction.Create(Guid.NewGuid(), Guid.NewGuid(), 10.0m, ValidCurrency);
            var invalidTags = new[] { "", "groceries" };

            // Act
            Action act = () => transaction.UpdateTags(invalidTags);

            // Assert
            act.Should().Throw<ValidationException>().WithMessage("*Tags cannot contain empty*");
        }

        [Fact]
        public void ChangeCurrency_WithValidCurrency_ShouldUpdateAndTimestamp()
        {
            // Arrange
            var transaction = Transaction.Create(Guid.NewGuid(), Guid.NewGuid(), 10.0m, ValidCurrency);
            var newCurrency = Currency.Create("EUR", "€", "Euro");

            // Act
            transaction.ChangeCurrency(newCurrency);

            // Assert
            transaction.Currency.Should().Be(newCurrency);
            transaction.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void ChangeCategory_WithValidId_ShouldUpdateAndTimestamp()
        {
            // Arrange
            var transaction = Transaction.Create(Guid.NewGuid(), Guid.NewGuid(), 10.0m, ValidCurrency);
            var newCategoryId = Guid.NewGuid();

            // Act
            transaction.ChangeCategory(newCategoryId);

            // Assert
            transaction.CategoryId.Should().Be(newCategoryId);
            transaction.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void UpdateType_ShouldUpdateTransactionTypeAndTimestamp()
        {
            // Arrange
            var transaction = Transaction.Create(Guid.NewGuid(), Guid.NewGuid(), 10.0m, ValidCurrency, TransactionType.Expense);

            // Act
            transaction.UpdateType(TransactionType.Income);

            // Assert
            transaction.Type.Should().Be(TransactionType.Income);
            transaction.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Create_WithNullTags_ShouldSetEmptyList()
        {
            var transaction = Transaction.Create(Guid.NewGuid(), Guid.NewGuid(), 100.0m, ValidCurrency, tags: null);
            transaction.Tags.Should().BeEmpty();
        }

        [Fact]
        public void Create_WithNullDescription_ShouldSetEmptyString()
        {
            var transaction = Transaction.Create(Guid.NewGuid(), Guid.NewGuid(), 100.0m, ValidCurrency, description: null);
            transaction.Description.Should().BeEmpty();
        }

        [Fact]
        public void Create_WithoutDate_ShouldSetCurrentUtc()
        {
            var transaction = Transaction.Create(Guid.NewGuid(), Guid.NewGuid(), 100.0m, ValidCurrency);
            transaction.Date.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void UpdateTags_WithNull_ShouldClearTags()
        {
            var transaction = Transaction.Create(Guid.NewGuid(), Guid.NewGuid(), 10.0m, ValidCurrency, tags: new[] { "test" });
            transaction.UpdateTags(null);
            transaction.Tags.Should().BeEmpty();
        }

        [Fact]
        public void UpdateTags_WithValidList_ShouldUpdateAndTimestamp()
        {
            var transaction = Transaction.Create(Guid.NewGuid(), Guid.NewGuid(), 10.0m, ValidCurrency);
            var newTags = new[] { "rent" };
            transaction.UpdateTags(newTags);
            transaction.Tags.Should().BeEquivalentTo(newTags);
            transaction.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void UpdateDate_ShouldChangeDateAndTimestamp()
        {
            var transaction = Transaction.Create(Guid.NewGuid(), Guid.NewGuid(), 10.0m, ValidCurrency);
            var newDate = DateTime.UtcNow.AddDays(-2);
            transaction.UpdateDate(newDate);
            transaction.Date.Should().BeCloseTo(newDate, TimeSpan.FromSeconds(1));
            transaction.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void UpdateType_WithSameValue_ShouldNotChangeUpdatedAt()
        {
            var transaction = Transaction.Create(Guid.NewGuid(), Guid.NewGuid(), 10.0m, ValidCurrency, TransactionType.Expense);
            transaction.UpdateType(TransactionType.Expense);
            transaction.UpdatedAt.Should().BeNull();
        }

        [Fact]
        public void Validate_WithMultipleInvalidFields_ShouldReturnAllErrors()
        {
            var errors = Transaction.Validate(Guid.Empty, Guid.Empty, -5, null, new string('X', 300), new[] { "", "food", "FOOD" });
            errors.Should().Contain(e => e.Contains("UserId is required"));
            errors.Should().Contain(e => e.Contains("CategoryId is required"));
            errors.Should().Contain(e => e.Contains("Amount must be greater than zero"));
            errors.Should().Contain(e => e.Contains("Currency is required"));
            errors.Should().Contain(e => e.Contains("Description cannot exceed"));
            errors.Should().Contain(e => e.Contains("Tags cannot contain empty"));
            errors.Should().Contain(e => e.Contains("Tags must be unique"));
        }
    }
}