using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Sqlite;


namespace HouseholdBudget.Core.Data
{
    /// <summary>
    /// Factory used by EF Core CLI to create BudgetDbContext during migrations.
    /// </summary>
    public class BudgetDbContextFactory : IDesignTimeDbContextFactory<BudgetDbContext>
    {
        public BudgetDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<BudgetDbContext>();
            optionsBuilder.UseSqlite("Data Source=household.db");

            return new BudgetDbContext(optionsBuilder.Options);
        }
    }
}
