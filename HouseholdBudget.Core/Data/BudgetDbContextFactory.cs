using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace HouseholdBudget.Core.Data
{
    /// <summary>
    /// Factory used by EF Core CLI to create BudgetDbContext during migrations.
    /// </summary>
    public class BudgetDbContextFactory : IDesignTimeDbContextFactory<BudgetDbContext>
    {
        public BudgetDbContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json")
                   .Build();

            var optionsBuilder = new DbContextOptionsBuilder<BudgetDbContext>();
            optionsBuilder.UseSqlServer(config.GetConnectionString("AzureSql"));

            return new BudgetDbContext(optionsBuilder.Options);
        }
    }
}
