using HouseholdBudget.Core.Models;
using HouseholdBudget.Core.UserData;

namespace HouseholdBudget.Core.Data
{
    /// <summary>
    /// Provides an abstraction for accessing and manipulating budgeting-related data,
    /// including users, transactions, categories, and persistence operations.
    /// </summary>
    public interface IBudgetRepository
    {
        /// <summary>
        /// Retrieves a user by their email address.
        /// </summary>
        /// <param name="email">The user's email address.</param>
        /// <returns>The user if found; otherwise, <c>null</c>.</returns>
        Task<User?> GetUserByEmailAsync(string email);

        /// <summary>
        /// Adds a new user to the persistence context.
        /// Changes must be explicitly committed using <see cref="SaveChangesAsync"/>.
        /// </summary>
        /// <param name="user">The user entity to add.</param>
        Task AddUserAsync(User user);

        /// <summary>
        /// Asynchronously retrieves all transactions associated with the specified user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>A collection of transactions belonging to the user.</returns>
        Task<IEnumerable<Transaction>> GetTransactionsByUserAsync(Guid userId);

        /// <summary>
        /// Asynchronously retrieves a transaction by its unique identifier.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Transaction?> GetTransactionByIdAsync(Guid id);

        /// <summary>
        /// Asynchronously retrieves all categories created by the specified user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>A collection of user-defined categories.</returns>
        Task<IEnumerable<Category>> GetCategoriesByUserAsync(Guid userId);

        /// <summary>
        /// Asynchronously retrieves all budget plans created by the specified user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>A collection of user-defined categories.</returns>
        Task<IEnumerable<BudgetPlan>> GetBudgetPlansByUserAsync(Guid userId);

        /// <summary>
        /// Asynchronously retrieves a budget plan by its unique identifier.
        /// </summary>
        /// <param name="planId"></param>
        /// <returns></returns>
        Task<BudgetPlan?> GetBudgetPlanByIdAsync(Guid planId);

        /// <summary>
        /// Adds a new transaction to the persistence context.
        /// Changes must be explicitly committed using <see cref="SaveChangesAsync"/>.
        /// </summary>
        /// <param name="transaction">The transaction to add.</param>
        Task AddTransactionAsync(Transaction transaction);

        /// <summary>
        /// Marks an existing transaction for update in the persistence context.
        /// Changes must be explicitly committed using <see cref="SaveChangesAsync"/>.
        /// </summary>
        /// <param name="transaction">The transaction to update.</param>
        Task UpdateTransactionAsync(Transaction transaction);

        /// <summary>
        /// Marks a transaction for deletion from the persistence context.
        /// Changes must be explicitly committed using <see cref="SaveChangesAsync"/>.
        /// </summary>
        /// <param name="transaction">The transaction to remove.</param>
        Task RemoveTransactionAsync(Transaction transaction);

        /// <summary>
        /// Adds a new category to the persistence context.
        /// Changes must be explicitly committed using <see cref="SaveChangesAsync"/>.
        /// </summary>
        /// <param name="category">The category to add.</param>
        Task AddCategoryAsync(Category category);

        /// <summary>
        /// Marks an existing category for update in the persistence context.
        /// Changes must be explicitly committed using <see cref="SaveChangesAsync"/>.
        /// </summary>
        /// <param name="category">The category to update.</param>
        Task UpdateCategoryAsync(Category category);

        /// <summary>
        /// Marks a category for deletion from the persistence context.
        /// Changes must be explicitly committed using <see cref="SaveChangesAsync"/>.
        /// </summary>
        /// <param name="category">The category to remove.</param>
        Task RemoveCategoryAsync(Category category);

        /// <summary>
        /// Adds a new budget plan to the persistence context.
        /// Changes must be explicitly committed using <see cref="SaveChangesAsync"/>.
        /// </summary>
        /// <param name="plan">The budget plan to add.</param>
        Task AddBudgetPlanAsync(BudgetPlan plan);

        /// <summary>
        /// Marks an existing budget plan for update in the persistence context.
        /// Changes must be explicitly committed using <see cref="SaveChangesAsync"/>.
        /// </summary>
        /// <param name="plan">The budget plan to update.</param>
        Task UpdateBudgetPlanAsync(BudgetPlan plan);

        /// <summary>
        /// Adds a new category budget plan to the persistence context.
        /// <param name="categoryPlan"/> must be associated with a budget plan.
        Task AddCategoryPlanAsync(CategoryBudgetPlan categoryPlan);

        /// <summary>
        /// Marks a budget plan for deletion from the persistence context.
        /// Changes must be explicitly committed using <see cref="SaveChangesAsync"/>.
        /// </summary>
        /// <param name="plan">The budget plan to remove.</param>
        Task RemoveBudgetPlanAsync(BudgetPlan plan);

        /// <summary>
        /// Asynchronously saves all pending changes to the underlying database.
        /// </summary>
        Task SaveChangesAsync();
    }
}
