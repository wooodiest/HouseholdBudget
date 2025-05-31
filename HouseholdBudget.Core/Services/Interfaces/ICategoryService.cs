using HouseholdBudget.Core.Models;

namespace HouseholdBudget.Core.Services.Interfaces
{
    /// <summary>
    /// Defines a contract for managing category data within a household budget system,
    /// including creation, retrieval, update, and deletion operations.
    /// </summary>
    public interface ICategoryService
    {
        /// <summary>
        /// Retrieves all categories associated with the currently authenticated user.
        /// </summary>
        /// <returns>A list of user-defined categories.</returns>
        Task<IEnumerable<Category>> GetUserCategoriesAsync();

        /// <summary>
        /// Creates a new category for the currently authenticated user.
        /// </summary>
        /// <param name="name">The name of the new category.</param>
        /// <returns>The newly created <see cref="Category"/> instance.</returns>
        Task<Category> CreateCategoryAsync(string name);

        /// <summary>
        /// Retrieves a category by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the category.</param>
        /// <returns>The <see cref="Category"/> if found; otherwise, null.</returns>
        Task<Category?> GetCategoryByIdAsync(Guid id);

        /// <summary>
        /// Retrieves a category with the specified name for the currently authenticated user.
        /// </summary>
        /// <param name="name">The case-insensitive name of the category to search for.</param>
        /// <returns>The matching <see cref="Category"/> if found; otherwise, null.</returns>
        Task<Category?> GetCategoryByNameAsync(string name);

        /// <summary>
        /// Checks whether a category with the specified ID exists.
        /// </summary>
        /// <param name="id">The category ID to check for existence.</param>
        /// <returns>True if the category exists; otherwise, false.</returns>
        Task<bool> CategoryExistsAsync(Guid id);

        /// <summary>
        /// Renames an existing category.
        /// </summary>
        /// <param name="categoryId">The ID of the category to rename.</param>
        /// <param name="newName">The new name for the category.</param>
        Task RenameCategoryAsync(Guid categoryId, string newName);

        /// <summary>
        /// Permanently removes a category from the system.
        /// </summary>
        /// <param name="categoryId">The ID of the category to delete.</param>
        Task DeleteCategoryAsync(Guid categoryId);
    }
}
