using FluentAssertions;
using HouseholdBudget.Core.Data;
using HouseholdBudget.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace HouseholdBudget.Tests.Data
{
    public class BudgetRepositoryTests
    {
        private BudgetDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<BudgetDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new BudgetDbContext(options);
        }

        private static Transaction CreateSampleTransaction(Guid userId)
        {
            return Transaction.Create(userId, Guid.NewGuid(), 100m, Currency.Create("USD", "$", "US Dollar"));
        }

        private static Category CreateSampleCategory(Guid userId)
        {
            return Category.Create(userId, "Groceries");
        }

        [Fact]
        public async Task AddTransactionAsync_ShouldAddAndRetrieveTransaction()
        {
            using var context = CreateInMemoryContext();
            var repository = new BudgetRepository(context);
            var userId = Guid.NewGuid();
            var transaction = CreateSampleTransaction(userId);

            await repository.AddTransactionAsync(transaction);
            await repository.SaveChangesAsync();
            var result = await repository.GetTransactionsByUserAsync(userId);

            result.Should().ContainSingle(t => t.Id == transaction.Id);
        }

        [Fact]
        public async Task UpdateTransactionAsync_ShouldModifyTransaction()
        {
            using var context = CreateInMemoryContext();
            var repository = new BudgetRepository(context);
            var transaction = CreateSampleTransaction(Guid.NewGuid());
            await repository.AddTransactionAsync(transaction);
            await repository.SaveChangesAsync();

            transaction.UpdateAmount(500);
            await repository.UpdateTransactionAsync(transaction);
            await repository.SaveChangesAsync();

            var updated = await context.Transactions.FindAsync(transaction.Id);

            updated!.Amount.Should().Be(500);
        }

        [Fact]
        public async Task RemoveTransactionAsync_ShouldRemoveTransaction()
        {
            using var context = CreateInMemoryContext();
            var repository = new BudgetRepository(context);
            var transaction = CreateSampleTransaction(Guid.NewGuid());
            await repository.AddTransactionAsync(transaction);
            await repository.SaveChangesAsync();

            await repository.RemoveTransactionAsync(transaction);
            await repository.SaveChangesAsync();

            var result = await context.Transactions.FindAsync(transaction.Id);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetTransactionsByUserAsync_ShouldReturnOnlyMatchingUserTransactions()
        {
            using var context = CreateInMemoryContext();
            var repository = new BudgetRepository(context);
            var userId = Guid.NewGuid();
            var t1 = CreateSampleTransaction(userId);
            var t2 = CreateSampleTransaction(Guid.NewGuid());

            await repository.AddTransactionAsync(t1);
            await repository.AddTransactionAsync(t2);
            await repository.SaveChangesAsync();

            var result = await repository.GetTransactionsByUserAsync(userId);

            result.Should().ContainSingle(t => t.UserId == userId);
        }

        [Fact]
        public async Task AddCategoryAsync_ShouldAddAndRetrieveCategory()
        {
            using var context = CreateInMemoryContext();
            var repository = new BudgetRepository(context);
            var userId = Guid.NewGuid();
            var category = CreateSampleCategory(userId);

            await repository.AddCategoryAsync(category);
            await repository.SaveChangesAsync();
            var result = await repository.GetCategoriesByUserAsync(userId);

            result.Should().ContainSingle(c => c.Id == category.Id);
        }

        [Fact]
        public async Task UpdateCategoryAsync_ShouldModifyCategory()
        {
            using var context = CreateInMemoryContext();
            var repository = new BudgetRepository(context);
            var category = CreateSampleCategory(Guid.NewGuid());
            await repository.AddCategoryAsync(category);
            await repository.SaveChangesAsync();

            category.Rename("Utilities");
            await repository.UpdateCategoryAsync(category);
            await repository.SaveChangesAsync();

            var updated = await context.Categories.FindAsync(category.Id);

            updated!.Name.Should().Be("Utilities");
        }

        [Fact]
        public async Task RemoveCategoryAsync_ShouldRemoveCategory()
        {
            using var context = CreateInMemoryContext();
            var repository = new BudgetRepository(context);
            var category = CreateSampleCategory(Guid.NewGuid());
            await repository.AddCategoryAsync(category);
            await repository.SaveChangesAsync();

            await repository.RemoveCategoryAsync(category);
            await repository.SaveChangesAsync();

            var result = await context.Categories.FindAsync(category.Id);

            result.Should().BeNull();
        }

        [Fact]
        public async Task SaveChangesAsync_ShouldCommitChanges()
        {
            using var context = CreateInMemoryContext();
            var repository = new BudgetRepository(context);
            var userId = Guid.NewGuid();
            var transaction = CreateSampleTransaction(userId);

            await repository.AddTransactionAsync(transaction);
            await repository.SaveChangesAsync();

            var exists = await context.Transactions.AnyAsync(t => t.Id == transaction.Id);
            exists.Should().BeTrue();
        }

        [Fact]
        public async Task GetTransactionsByUserAsync_ShouldReturnEmptyIfNoneExist()
        {
            using var context = CreateInMemoryContext();
            var repository = new BudgetRepository(context);
            var result = await repository.GetTransactionsByUserAsync(Guid.NewGuid());
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task AddTransactionAndCategory_ShouldPersistRelation()
        {
            using var context = CreateInMemoryContext();
            var repository = new BudgetRepository(context);
            var userId = Guid.NewGuid();
            var category = CreateSampleCategory(userId);
            await repository.AddCategoryAsync(category);
            await repository.SaveChangesAsync();

            var transaction = Transaction.Create(userId, category.Id, 200m, Currency.Create("EUR", "€", "Euro"));
            await repository.AddTransactionAsync(transaction);
            await repository.SaveChangesAsync();

            var result = await repository.GetTransactionsByUserAsync(userId);
            result.Should().ContainSingle(t => t.CategoryId == category.Id);
        }

        [Fact]
        public async Task GetCategoriesByUserAsync_ShouldReturnOnlyUserCategories()
        {
            using var context = CreateInMemoryContext();
            var repository = new BudgetRepository(context);
            var userId = Guid.NewGuid();
            var otherUser = Guid.NewGuid();

            var cat1 = CreateSampleCategory(userId);
            var cat2 = CreateSampleCategory(otherUser);

            await repository.AddCategoryAsync(cat1);
            await repository.AddCategoryAsync(cat2);
            await repository.SaveChangesAsync();

            var result = await repository.GetCategoriesByUserAsync(userId);
            result.Should().OnlyContain(c => c.UserId == userId);
        }
    }
}