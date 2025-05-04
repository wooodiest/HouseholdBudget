using HouseholdBudget.Core.Data;
using HouseholdBudget.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HouseholdBudget.Desktop
{
    public static class AppBootstrapper
    {
        public static ServiceProvider Configure()
        {
            var services = new ServiceCollection();

            string dbPath = "householdBudget.db";
            services.AddSingleton<IDatabaseManager>(provider =>
                new LocalDatabaseManager(dbPath));

            //services.AddSingleton<IUserContext, UserContext>();
            services.AddSingleton<ICategoryService, CategoryService>();
            services.AddSingleton<ITransactionService, TransactionService>();

            return services.BuildServiceProvider();
        }
    }

}
