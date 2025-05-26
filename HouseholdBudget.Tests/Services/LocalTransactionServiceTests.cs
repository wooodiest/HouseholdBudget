using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using HouseholdBudget.Core.Data;
using HouseholdBudget.Core.Events.Transactions;
using HouseholdBudget.Core.Models;
using HouseholdBudget.Core.Services.Interfaces;
using HouseholdBudget.Core.Services.Local;
using HouseholdBudget.Core.UserData;
using Moq;
using Xunit;

namespace HouseholdBudget.Tests.Services
{
    public class LocalTransactionServiceTests
    {
        private readonly Mock<IBudgetRepository> _repositoryMock = new();
        private readonly Mock<IUserSessionService> _userSessionMock = new();
        private readonly Mock<ITransactionEventPublisher> _eventPublisherMock = new();
        private readonly Mock<ICategoryService> _categoryServiceMock = new();

        private readonly User _testUser = new()
        {
            Id = Guid.NewGuid(),
            Name = "Test User",
            Email = "test@example.com",
            PasswordHash = "hashed",
            DefaultCurrencyCode = "USD"
        };

        private LocalTransactionService CreateService(User? user = null)
        {
            _userSessionMock.Setup(x => x.GetUser()).Returns(user);
            return new LocalTransactionService(
                _repositoryMock.Object,
                _userSessionMock.Object,
                _eventPublisherMock.Object,
                _categoryServiceMock.Object);
        }


        [Fact]
        public async Task CreateAsync_ShouldAddTransaction_AndPublishEvent()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var currency = Currency.Create("USD", "$", "US Dollar");
            var service = CreateService(_testUser);

            // Act
            var transaction = await service.CreateAsync(categoryId, 100, currency, TransactionType.Expense);

            // Assert
            transaction.Should().NotBeNull();
            transaction.Amount.Should().Be(100);
            transaction.Currency.Code.Should().Be("USD");
            _repositoryMock.Verify(r => r.AddTransactionAsync(It.IsAny<Transaction>()), Times.Once);
            _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            _eventPublisherMock.Verify(e => e.PublishAsync(It.IsAny<TransactionCreated>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveTransaction_AndPublishEvent()
        {
            // Arrange
            var service = CreateService(_testUser);
            var transaction = Transaction.Create(_testUser.Id, Guid.NewGuid(), 50, Currency.Create("USD", "$", "US Dollar"));
            _repositoryMock.Setup(r => r.GetTransactionsByUserAsync(_testUser.Id))
                           .ReturnsAsync(new List<Transaction> { transaction });

            // Act
            await service.DeleteAsync(transaction.Id);

            // Assert
            _repositoryMock.Verify(r => r.RemoveTransactionAsync(transaction), Times.Once);
            _eventPublisherMock.Verify(e => e.PublishAsync(It.IsAny<TransactionDeleted>()), Times.Once);
        }

        [Fact]
        public async Task GetAsync_WithNullFilter_ShouldReturnAllTransactions()
        {
            // Arrange
            var service = CreateService(_testUser);
            var transaction = Transaction.Create(_testUser.Id, Guid.NewGuid(), 25, Currency.Create("USD", "$", "US Dollar"));
            _repositoryMock.Setup(r => r.GetTransactionsByUserAsync(_testUser.Id))
                           .ReturnsAsync(new List<Transaction> { transaction });

            // Act
            var result = await service.GetAsync();

            // Assert
            result.Should().ContainSingle();
        }

        [Fact]
        public async Task UpdateAmountAsync_ShouldUpdateTransaction()
        {
            // Arrange
            var service = CreateService(_testUser);
            var transaction = Transaction.Create(_testUser.Id, Guid.NewGuid(), 10, Currency.Create("USD", "$", "US Dollar"));
            _repositoryMock.Setup(r => r.GetTransactionsByUserAsync(_testUser.Id))
                           .ReturnsAsync(new List<Transaction> { transaction });

            // Act
            await service.UpdateAmountAsync(transaction.Id, 99);

            // Assert
            transaction.Amount.Should().Be(99);
            _repositoryMock.Verify(r => r.UpdateTransactionAsync(transaction), Times.Once);
        }

        [Fact]
        public async Task UpdateDescriptionAsync_ShouldUpdateDescription()
        {
            // Arrange
            var service = CreateService(_testUser);
            var transaction = Transaction.Create(_testUser.Id, Guid.NewGuid(), 10, Currency.Create("USD", "$", "US Dollar"), description: "old");
            _repositoryMock.Setup(r => r.GetTransactionsByUserAsync(_testUser.Id))
                           .ReturnsAsync(new List<Transaction> { transaction });

            // Act
            await service.UpdateDescriptionAsync(transaction.Id, "new");

            // Assert
            transaction.Description.Should().Be("new");
            _repositoryMock.Verify(r => r.UpdateTransactionAsync(transaction), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCorrectTransaction()
        {
            var service = CreateService(_testUser);
            var transaction = Transaction.Create(_testUser.Id, Guid.NewGuid(), 77, Currency.Create("USD", "$", "US Dollar"));
            _repositoryMock.Setup(r => r.GetTransactionsByUserAsync(_testUser.Id))
                           .ReturnsAsync(new List<Transaction> { transaction });

            var result = await service.GetByIdAsync(transaction.Id);

            result.Should().NotBeNull();
            result!.Id.Should().Be(transaction.Id);
        }

        [Fact]
        public async Task UpdateTagsAsync_ShouldUpdateTags()
        {
            var service = CreateService(_testUser);
            var transaction = Transaction.Create(_testUser.Id, Guid.NewGuid(), 10, Currency.Create("USD", "$", "US Dollar"), tags: new[] { "old" });
            _repositoryMock.Setup(r => r.GetTransactionsByUserAsync(_testUser.Id))
                           .ReturnsAsync(new List<Transaction> { transaction });

            await service.UpdateTagsAsync(transaction.Id, new[] { "new1", "new2" });

            transaction.Tags.Should().BeEquivalentTo(new[] { "new1", "new2" });
        }

        [Fact]
        public async Task UpdateTypeAsync_ShouldChangeType()
        {
            var service = CreateService(_testUser);
            var transaction = Transaction.Create(_testUser.Id, Guid.NewGuid(), 10, Currency.Create("USD", "$", "US Dollar"), type: TransactionType.Expense);
            _repositoryMock.Setup(r => r.GetTransactionsByUserAsync(_testUser.Id))
                           .ReturnsAsync(new List<Transaction> { transaction });

            await service.UpdateTypeAsync(transaction.Id, TransactionType.Income);

            transaction.Type.Should().Be(TransactionType.Income);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowIfUserNotAuthenticated()
        {
            var service = CreateService(user: null);

            Func<Task> act = () => service.CreateAsync(
                Guid.NewGuid(),
                10,
                Currency.Create("USD", "$", "US Dollar"),
                TransactionType.Expense);

            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("User is not authenticated.");
        }


        [Fact]
        public async Task GetAsync_ShouldApplyDateFilter()
        {
            var service = CreateService(_testUser);
            var today = DateTime.UtcNow.Date;
            var match = Transaction.Create(_testUser.Id, Guid.NewGuid(), 100, Currency.Create("USD", "$", "US Dollar"), date: today);
            var other = Transaction.Create(_testUser.Id, Guid.NewGuid(), 50, Currency.Create("USD", "$", "US Dollar"), date: today.AddDays(-1));
            _repositoryMock.Setup(r => r.GetTransactionsByUserAsync(_testUser.Id))
                           .ReturnsAsync(new List<Transaction> { match, other });

            var result = await service.GetAsync(new TransactionFilter { Date = today });

            result.Should().ContainSingle().Which.Id.Should().Be(match.Id);
        }

        [Fact]
        public async Task GetAsync_ShouldApplyAmountRangeFilter()
        {
            var service = CreateService(_testUser);
            var low = Transaction.Create(_testUser.Id, Guid.NewGuid(), 10, Currency.Create("USD", "$", "US Dollar"));
            var mid = Transaction.Create(_testUser.Id, Guid.NewGuid(), 50, Currency.Create("USD", "$", "US Dollar"));
            var high = Transaction.Create(_testUser.Id, Guid.NewGuid(), 100, Currency.Create("USD", "$", "US Dollar"));
            _repositoryMock.Setup(r => r.GetTransactionsByUserAsync(_testUser.Id))
                           .ReturnsAsync(new List<Transaction> { low, mid, high });

            var result = await service.GetAsync(new TransactionFilter { MinAmount = 30, MaxAmount = 80 });

            result.Should().ContainSingle().Which.Id.Should().Be(mid.Id);
        }

        [Fact]
        public async Task UpdateAmountAsync_ShouldThrow_WhenTransactionNotFound()
        {
            var service = CreateService(_testUser);
            _repositoryMock.Setup(r => r.GetTransactionsByUserAsync(_testUser.Id))
                           .ReturnsAsync(new List<Transaction>());

            Func<Task> act = () => service.UpdateAmountAsync(Guid.NewGuid(), 50);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Transaction not found.");
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrow_WhenTransactionNotFound()
        {
            var service = CreateService(_testUser);
            _repositoryMock.Setup(r => r.GetTransactionsByUserAsync(_testUser.Id))
                           .ReturnsAsync(new List<Transaction>());

            Func<Task> act = () => service.DeleteAsync(Guid.NewGuid());

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Transaction not found.");
        }


        [Fact]
        public async Task UpdateDescriptionAsync_ShouldTriggerTransactionUpdatedEvent()
        {
            var service = CreateService(_testUser);
            var transaction = Transaction.Create(_testUser.Id, Guid.NewGuid(), 20, Currency.Create("USD", "$", "US Dollar"), description: "old");
            _repositoryMock.Setup(r => r.GetTransactionsByUserAsync(_testUser.Id))
                           .ReturnsAsync(new List<Transaction> { transaction });

            await service.UpdateDescriptionAsync(transaction.Id, "new");

            _eventPublisherMock.Verify(p => p.PublishAsync(It.Is<TransactionUpdated>(e => e.Transaction.Id == transaction.Id)), Times.Once);
        }

        [Fact]
        public async Task GetAsync_ShouldApplyCurrencyFilter()
        {
            var service = CreateService(_testUser);
            var usd = Currency.Create("USD", "$", "US Dollar");
            var eur = Currency.Create("EUR", "€", "Euro");

            var usdTxn = Transaction.Create(_testUser.Id, Guid.NewGuid(), 20, usd);
            var eurTxn = Transaction.Create(_testUser.Id, Guid.NewGuid(), 30, eur);

            _repositoryMock.Setup(r => r.GetTransactionsByUserAsync(_testUser.Id))
                           .ReturnsAsync(new List<Transaction> { usdTxn, eurTxn });

            var result = await service.GetAsync(new TransactionFilter { Currency = usd });

            result.Should().ContainSingle().Which.Currency.Code.Should().Be("USD");
        }

        [Fact]
        public async Task GetAsync_ShouldApplyCategoryTypeFilter()
        {
            var service = CreateService(_testUser);
            var categoryId = Guid.NewGuid();
            var category = Category.Create(_testUser.Id, "Salary", CategoryType.Income);

            var txn = Transaction.Create(_testUser.Id, categoryId, 100, Currency.Create("USD", "$", "US Dollar"));
            _repositoryMock.Setup(r => r.GetTransactionsByUserAsync(_testUser.Id))
                           .ReturnsAsync(new List<Transaction> { txn });

            _categoryServiceMock.Setup(c => c.GetCategoryByIdAsync(categoryId))
                                .ReturnsAsync(category);

            var result = await service.GetAsync(new TransactionFilter { CategoryType = CategoryType.Income });

            result.Should().ContainSingle().Which.Id.Should().Be(txn.Id);
        }

        [Fact]
        public async Task UpdateDateAsync_ShouldResortCache()
        {
            var service = CreateService(_testUser);
            var oldDate = DateTime.UtcNow.AddDays(-1);
            var newDate = DateTime.UtcNow;

            var txn1 = Transaction.Create(_testUser.Id, Guid.NewGuid(), 10, Currency.Create("USD", "$", "US Dollar"), date: oldDate);
            var txn2 = Transaction.Create(_testUser.Id, Guid.NewGuid(), 10, Currency.Create("USD", "$", "US Dollar"), date: newDate);

            _repositoryMock.Setup(r => r.GetTransactionsByUserAsync(_testUser.Id))
                           .ReturnsAsync(new List<Transaction> { txn1, txn2 });

            await service.UpdateDateAsync(txn1.Id, newDate.AddDays(1));
            var list = (await service.GetAsync()).ToList();

            list.First().Id.Should().Be(txn2.Id);
            list.Last().Id.Should().Be(txn1.Id);
        }

        [Fact]
        public async Task GetAsync_ShouldUseCachedTransactions_AfterInitialLoad()
        {
            var service = CreateService(_testUser);

            _repositoryMock.Setup(r => r.GetTransactionsByUserAsync(_testUser.Id))
                           .ReturnsAsync(new List<Transaction>());

            await service.GetAsync();

            await service.GetAsync();

            _repositoryMock.Verify(r => r.GetTransactionsByUserAsync(_testUser.Id), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ShouldAddToCache_SortedByDate()
        {
            var service = CreateService(_testUser);
            var usd = Currency.Create("USD", "$", "US Dollar");

            _repositoryMock.Setup(r => r.GetTransactionsByUserAsync(_testUser.Id))
                           .ReturnsAsync(new List<Transaction>());

            await service.GetAsync();

            var t1 = await service.CreateAsync(Guid.NewGuid(), 10, usd, TransactionType.Expense, date: DateTime.UtcNow.AddDays(-2));
            var t2 = await service.CreateAsync(Guid.NewGuid(), 10, usd, TransactionType.Expense, date: DateTime.UtcNow.AddDays(-1));

            var result = await service.GetAsync();

            result.ElementAt(0).Id.Should().Be(t1.Id);
            result.ElementAt(1).Id.Should().Be(t2.Id);
        }

        [Fact]
        public async Task UpdateTagsAsync_ShouldClearTags_WhenPassedNull()
        {
            var service = CreateService(_testUser);
            var txn = Transaction.Create(_testUser.Id, Guid.NewGuid(), 10, Currency.Create("USD", "$", "US Dollar"), tags: new[] { "food" });

            _repositoryMock.Setup(r => r.GetTransactionsByUserAsync(_testUser.Id))
                           .ReturnsAsync(new List<Transaction> { txn });

            await service.UpdateTagsAsync(txn.Id, null);

            txn.Tags.Should().BeEmpty();
        }

        [Fact]
        public async Task UpdateDescriptionAsync_ShouldThrow_WhenTooLong()
        {
            var service = CreateService(_testUser);
            var txn = Transaction.Create(_testUser.Id, Guid.NewGuid(), 10, Currency.Create("USD", "$", "US Dollar"), description: "Valid");

            _repositoryMock.Setup(r => r.GetTransactionsByUserAsync(_testUser.Id))
                           .ReturnsAsync(new List<Transaction> { txn });

            var tooLong = new string('x', Transaction.MaxDescriptionLength + 1);

            Func<Task> act = () => service.UpdateDescriptionAsync(txn.Id, tooLong);

            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task UpdateCurrencyAsync_ShouldThrow_WhenCurrencyIsNull()
        {
            var service = CreateService(_testUser);
            var txn = Transaction.Create(_testUser.Id, Guid.NewGuid(), 10, Currency.Create("USD", "$", "US Dollar"));

            _repositoryMock.Setup(r => r.GetTransactionsByUserAsync(_testUser.Id))
                           .ReturnsAsync(new List<Transaction> { txn });

            await Assert.ThrowsAsync<ValidationException>(() =>
                service.UpdateCurrencyAsync(txn.Id, null!));
        }

        [Fact]
        public async Task GetAsync_ShouldReturnEmptyList_WhenUserHasNoTransactions()
        {
            var service = CreateService(_testUser);

            _repositoryMock.Setup(r => r.GetTransactionsByUserAsync(_testUser.Id))
                           .ReturnsAsync(new List<Transaction>());

            var result = await service.GetAsync();

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenTransactionDoesNotExist()
        {
            var service = CreateService(_testUser);
            _repositoryMock.Setup(r => r.GetTransactionsByUserAsync(_testUser.Id))
                           .ReturnsAsync(new List<Transaction>());

            var result = await service.GetByIdAsync(Guid.NewGuid());

            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateCategoryAsync_ShouldThrow_WhenCategoryIdIsEmpty()
        {
            var service = CreateService(_testUser);
            var txn = Transaction.Create(_testUser.Id, Guid.NewGuid(), 10, Currency.Create("USD", "$", "US Dollar"));

            _repositoryMock.Setup(r => r.GetTransactionsByUserAsync(_testUser.Id))
                           .ReturnsAsync(new List<Transaction> { txn });

            Func<Task> act = () => service.UpdateCategoryAsync(txn.Id, Guid.Empty);

            await act.Should().ThrowAsync<ValidationException>().WithMessage("*CategoryId is required*");
        }

        [Fact]
        public async Task UpdateAmountAsync_ShouldThrow_WhenAmountIsZero()
        {
            var service = CreateService(_testUser);
            var txn = Transaction.Create(_testUser.Id, Guid.NewGuid(), 10, Currency.Create("USD", "$", "US Dollar"));

            _repositoryMock.Setup(r => r.GetTransactionsByUserAsync(_testUser.Id))
                           .ReturnsAsync(new List<Transaction> { txn });

            Func<Task> act = () => service.UpdateAmountAsync(txn.Id, 0);

            await act.Should().ThrowAsync<ValidationException>().WithMessage("*greater than zero*");
        }

    }
}
