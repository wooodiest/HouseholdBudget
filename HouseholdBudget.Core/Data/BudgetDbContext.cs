using HouseholdBudget.Core.Models;
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
        /// Gets or sets the collection of transactions in the database.
        /// </summary>
        public DbSet<Transaction> Transactions => Set<Transaction>();

        /// <summary>
        /// Gets or sets the collection of categories defined by users.
        /// </summary>
        public DbSet<Category> Categories => Set<Category>();

        /// <summary>
        /// Gets or sets the collection of supported currencies.
        /// </summary>
        public DbSet<Currency> Currencies => Set<Currency>();

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
