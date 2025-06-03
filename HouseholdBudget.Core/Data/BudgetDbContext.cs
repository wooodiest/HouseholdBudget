using HouseholdBudget.Core.Models;
using HouseholdBudget.Core.UserData;
using Microsoft.EntityFrameworkCore;

namespace HouseholdBudget.Core.Data
{
    /// <summary>
    /// Represents the Entity Framework Core database context for the household budgeting application.
    /// It provides access to all entities in the domain model and handles database configuration.
    /// </summary>
    public class BudgetDbContext : DbContext
    {
        /// <summary>
        /// Gets or sets the collection of users in the database.
        /// </summary>
        public DbSet<User> Users => Set<User>();

        /// <summary>
        /// Gets or sets the collection of transactions in the database.
        /// </summary>
        public DbSet<Transaction> Transactions => Set<Transaction>();

        /// <summary>
        /// Gets or sets the collection of categories defined by users.
        /// </summary>
        public DbSet<Category> Categories => Set<Category>();

        /// <summary>
        /// Gets or sets the collection of budget plans in the database.
        /// </summary>
        public DbSet<BudgetPlan> BudgetPlans => Set<BudgetPlan>();

        /// <summary>
        /// Gets or sets the collection of category budget plans in the database.
        /// </summary>
        public DbSet<CategoryBudgetPlan> CategoryBudgetPlans => Set<CategoryBudgetPlan>();

        /// <summary>
        /// Initializes a new instance of the <see cref="BudgetDbContext"/> class
        /// using the specified options.
        /// </summary>
        /// <param name="options">The options to configure the context.</param>
        public BudgetDbContext(DbContextOptions<BudgetDbContext> options)
            : base(options) { }

        /// <summary>
        /// Configures the EF Core model. This method is called by the framework during model creation.
        /// </summary>
        /// <param name="modelBuilder">The model builder used to configure the entity model.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
