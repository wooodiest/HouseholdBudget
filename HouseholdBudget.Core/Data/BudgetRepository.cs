using HouseholdBudget.Core.Models;
using HouseholdBudget.Core.UserData;
using Microsoft.EntityFrameworkCore;

namespace HouseholdBudget.Core.Data
{
    /// <summary>
    /// Provides a concrete implementation of <see cref="IBudgetRepository"/>,
    /// enabling data access and manipulation for users, transactions, and categories
    /// using Entity Framework Core.
    /// </summary>
    public class BudgetRepository : IBudgetRepository
    {
        private readonly BudgetDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="BudgetRepository"/> class.
        /// </summary>
        /// <param name="context">The EF Core database context.</param>
        public BudgetRepository(BudgetDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        /// <inheritdoc/>
        public async Task AddUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Transaction>> GetTransactionsByUserAsync(Guid userId) =>
            await _context.Transactions
                .Include(t => t.Currency)
                .Where(t => t.UserId == userId)
                .ToListAsync();

        /// <inheritdoc/>
        public async Task<IEnumerable<Category>> GetCategoriesByUserAsync(Guid userId) =>
            await _context.Categories
                .Where(c => c.UserId == userId)
                .ToListAsync();

        /// <inheritdoc/>
        public async Task<IEnumerable<BudgetPlan>> GetBudgetPlansByUserAsync(Guid userId)
        {
            return await _context.BudgetPlans
                .Include(p => p.CategoryPlans)
                .Where(p => p.UserId == userId)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task AddTransactionAsync(Transaction transaction) =>
            await _context.Transactions.AddAsync(transaction);

        /// <inheritdoc/>
        public async Task UpdateTransactionAsync(Transaction transaction)
        {
            _context.Transactions.Update(transaction);
            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task RemoveTransactionAsync(Transaction transaction)
        {
            _context.Transactions.Remove(transaction);
            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task AddCategoryAsync(Category category) =>
            await _context.Categories.AddAsync(category);

        /// <inheritdoc/>
        public async Task UpdateCategoryAsync(Category category)
        {
            _context.Categories.Update(category);
            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task RemoveCategoryAsync(Category category)
        {
            _context.Categories.Remove(category);
            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task AddBudgetPlanAsync(BudgetPlan plan)
        {
            await _context.BudgetPlans.AddAsync(plan);
        }

        /// <inheritdoc/>
        public async Task UpdateBudgetPlanAsync(BudgetPlan plan)
        {
            _context.BudgetPlans.Update(plan);
            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task RemoveBudgetPlanAsync(BudgetPlan plan)
        {
            _context.BudgetPlans.Remove(plan);
            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task SaveChangesAsync() =>
            await _context.SaveChangesAsync();
    }
}
