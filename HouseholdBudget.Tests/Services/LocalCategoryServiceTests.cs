using FluentAssertions;
using HouseholdBudget.Core.Data;
using HouseholdBudget.Core.Models;
using HouseholdBudget.Core.Services;
using HouseholdBudget.Core.UserData;
using Moq;

namespace HouseholdBudget.Tests.Services
{
    public class LocalCategoryServiceTests
    {
        private readonly Mock<IBudgetRepository> _repositoryMock = new();
        private readonly Mock<IUserSessionService> _sessionMock = new();
        private readonly Guid _userId = Guid.NewGuid();
        private readonly User _user;

        public LocalCategoryServiceTests()
        {
            _user = new User {
                Id = _userId,
                Name = "Test User",
                Email = "test@example.com",
                PasswordHash = "hashed",
                DefaultCurrencyCode = "USD"
            };

            _sessionMock.Setup(s => s.GetUser()).Returns(_user);
        }

        [Fact]
        public async Task CreateCategoryAsync_ShouldAddAndReturnCategory()
        {
            var service = new LocalCategoryService(_repositoryMock.Object, _sessionMock.Object);

            var result = await service.CreateCategoryAsync("Food", CategoryType.Expense);

            result.Name.Should().Be("Food");
            result.Type.Should().Be(CategoryType.Expense);
            result.UserId.Should().Be(_userId);

            _repositoryMock.Verify(r => r.AddCategoryAsync(It.IsAny<Category>()), Times.Once);
            _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetUserCategoriesAsync_ShouldReturnCachedOnSecondCall()
        {
            var categories = new List<Category> {
                Category.Create(_userId, "Transport", CategoryType.Expense)
            };

            _repositoryMock.Setup(r => r.GetCategoriesByUserAsync(_userId))
                .ReturnsAsync(categories);

            var service = new LocalCategoryService(_repositoryMock.Object, _sessionMock.Object);

            var first = await service.GetUserCategoriesAsync();
            var second = await service.GetUserCategoriesAsync();

            first.Should().BeEquivalentTo(second);
            _repositoryMock.Verify(r => r.GetCategoriesByUserAsync(_userId), Times.Once);
        }

        [Fact]
        public async Task RenameCategoryAsync_ShouldUpdateName()
        {
            var category = Category.Create(_userId, "Old Name", CategoryType.Expense);
            _repositoryMock.Setup(r => r.GetCategoriesByUserAsync(_userId))
                .ReturnsAsync(new List<Category> { category });

            var service = new LocalCategoryService(_repositoryMock.Object, _sessionMock.Object);
            await service.RenameCategoryAsync(category.Id, "New Name");

            category.Name.Should().Be("New Name");
            _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task RenameCategoryAsync_ShouldThrowIfNotFound()
        {
            _repositoryMock.Setup(r => r.GetCategoriesByUserAsync(_userId))
                .ReturnsAsync(new List<Category>());

            var service = new LocalCategoryService(_repositoryMock.Object, _sessionMock.Object);

            Func<Task> act = async () => await service.RenameCategoryAsync(Guid.NewGuid(), "Updated");

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Category not found.");
        }

        [Fact]
        public async Task DeleteCategoryAsync_ShouldRemoveCategory()
        {
            var category = Category.Create(_userId, "Bills", CategoryType.Expense);
            _repositoryMock.Setup(r => r.GetCategoriesByUserAsync(_userId))
                .ReturnsAsync(new List<Category> { category });

            var service = new LocalCategoryService(_repositoryMock.Object, _sessionMock.Object);
            await service.DeleteCategoryAsync(category.Id);

            _repositoryMock.Verify(r => r.RemoveCategoryAsync(category), Times.Once);
            _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteCategoryAsync_ShouldThrowIfNotFound()
        {
            _repositoryMock.Setup(r => r.GetCategoriesByUserAsync(_userId))
                .ReturnsAsync(new List<Category>());

            var service = new LocalCategoryService(_repositoryMock.Object, _sessionMock.Object);

            Func<Task> act = async () => await service.DeleteCategoryAsync(Guid.NewGuid());

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Category not found.");
        }

        [Fact]
        public async Task CategoryExistsAsync_ShouldReturnTrueIfExists()
        {
            var category = Category.Create(_userId, "Health", CategoryType.Expense);
            _repositoryMock.Setup(r => r.GetCategoriesByUserAsync(_userId))
                .ReturnsAsync(new List<Category> { category });

            var service = new LocalCategoryService(_repositoryMock.Object, _sessionMock.Object);
            var exists = await service.CategoryExistsAsync(category.Id);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task GetCategoryByNameAsync_ShouldReturnMatchingCategory()
        {
            var category = Category.Create(_userId, "Groceries", CategoryType.Expense);
            _repositoryMock.Setup(r => r.GetCategoriesByUserAsync(_userId))
                .ReturnsAsync(new List<Category> { category });

            var service = new LocalCategoryService(_repositoryMock.Object, _sessionMock.Object);
            var found = await service.GetCategoryByNameAsync("groceries");

            found.Should().NotBeNull();
            found!.Name.Should().Be("Groceries");
        }

        [Fact]
        public async Task GetCategoryByNameAsync_ShouldReturnNull_WhenNotFound()
        {
            _repositoryMock.Setup(r => r.GetCategoriesByUserAsync(_userId))
                .ReturnsAsync(new List<Category>());

            var service = new LocalCategoryService(_repositoryMock.Object, _sessionMock.Object);
            var found = await service.GetCategoryByNameAsync("NonExistent");

            found.Should().BeNull();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task GetCategoryByNameAsync_ShouldThrow_WhenNameIsInvalid(string name)
        {
            var service = new LocalCategoryService(_repositoryMock.Object, _sessionMock.Object);

            Func<Task> act = async () => await service.GetCategoryByNameAsync(name!);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*Category name cannot be null or whitespace*");
        }

        [Fact]
        public async Task CategoryExistsAsync_ShouldReturnFalse_WhenCategoryNotFound()
        {
            _repositoryMock.Setup(r => r.GetCategoriesByUserAsync(_userId))
                .ReturnsAsync(new List<Category>());

            var service = new LocalCategoryService(_repositoryMock.Object, _sessionMock.Object);
            var exists = await service.CategoryExistsAsync(Guid.NewGuid());

            exists.Should().BeFalse();
        }


        [Fact]
        public void EnsureAuthenticated_ShouldThrowIfNoUser()
        {
            _sessionMock.Setup(s => s.GetUser()).Returns((User?)null);
            var service = new LocalCategoryService(_repositoryMock.Object, _sessionMock.Object);

            Func<Task> act = async () => await service.GetUserCategoriesAsync();
            act.Should().ThrowAsync<InvalidOperationException>();
        }
    }
}
