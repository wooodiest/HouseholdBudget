﻿using HouseholdBudget.Core.Data;
using HouseholdBudget.Core.Models;
using HouseholdBudget.Core.Services.Interfaces;
using HouseholdBudget.Core.UserData;

namespace HouseholdBudget.Core.Services.Local
{
    /// <summary>
    /// Local implementation of <see cref="ICategoryService"/> that interacts with 
    /// a repository and user session context to manage category data.
    /// </summary>
    public class LocalCategoryService : ICategoryService
    {
        private readonly IBudgetRepository _repository;
        private readonly IUserSessionService _userSession;

        /// <summary>
        /// Cached list of user categories to minimize redundant data access during a session.
        /// This cache is invalidated after changes like creation, update, or deletion.
        /// </summary>
        private List<Category> _categories = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalCategoryService"/> class.
        /// </summary>
        /// <param name="repository">The repository responsible for data persistence.</param>
        /// <param name="userSessionContext">The service that provides current user context.</param>
        public LocalCategoryService(IBudgetRepository repository, IUserSessionService userSessionContext)
        {
            _repository = repository;
            _userSession = userSessionContext;
        }

        /// <inheritdoc />
        public async Task InitAsync()
        {
            EnsureAuthenticated();
            _categories.Clear();
            _categories = [.. await _repository.GetCategoriesByUserAsync(_userSession.GetUser()!.Id)];
        }

        /// <inheritdoc />
        public async Task<bool> CategoryExistsAsync(Guid id)
        {
            var category = await GetCategoryByIdAsync(id);
            return category != null;
        }

        /// <inheritdoc />
        public async Task<Category> CreateCategoryAsync(string name)
        {
            var user     = EnsureAuthenticated();
            var category = Category.Create(user.Id, name);

            _categories.Add(category);

            await _repository.AddCategoryAsync(category);
            await _repository.SaveChangesAsync();

            return category;
        }

        /// <inheritdoc />
        public async Task DeleteCategoryAsync(Guid categoryId)
        {
            var category = await _repository.GetCategoryByIdAsync(categoryId);
            if (category == null)
                throw new InvalidOperationException("Category not found.");

            _categories.Remove(category);
            await _repository.RemoveCategoryAsync(category);
            await _repository.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<Category?> GetCategoryByIdAsync(Guid id)
        {
            var categories = await GetUserCategoriesAsync();
            return categories.FirstOrDefault(c => c.Id == id);
        }

        /// <inheritdoc />
        public async Task<Category?> GetCategoryByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Category name cannot be null or whitespace.", nameof(name));

            var categories = await GetUserCategoriesAsync();
            return categories
                .FirstOrDefault(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        /// <inheritdoc />
        public Task<IEnumerable<Category>> GetUserCategoriesAsync() => Task.FromResult(_categories.AsEnumerable());

        /// <inheritdoc />
        public async Task RenameCategoryAsync(Guid categoryId, string newName)
        {
            var category = await _repository.GetCategoryByIdAsync(categoryId);
            if (category == null)
                throw new InvalidOperationException("Category not found.");

            category.Rename(newName);
            await _repository.UpdateCategoryAsync(category);
            await _repository.SaveChangesAsync();
        }

        /// <summary>
        /// Ensures that the current session is associated with an authenticated user.
        /// Throws an exception if not authenticated.
        /// </summary>
        /// <returns>The authenticated <see cref="User"/>.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the user is not authenticated.</exception>
        private User EnsureAuthenticated()
        {
            return _userSession.GetUser() ?? throw new InvalidOperationException("User is not authenticated.");
        }
    }
}
