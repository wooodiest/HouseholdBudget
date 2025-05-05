using HouseholdBudget.Core.Core;
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

            services.AddSingleton<IAppConfiguration, AppConfiguration>();
            services.AddSingleton<IUserStorage, UserStorage>();
            services.AddSingleton<ILoginService, LoginService>();
            services.AddSingleton<IDatabaseManager, LocalDatabaseManager>();
            services.AddSingleton<IUserContext, UserContext>();

            services.AddSingleton<IExchangeRateProvider, DummyExchangeRateProvider>();
            services.AddSingleton<IExchangeRateService, ExchangeRateService>();
            services.AddSingleton<ICategoryService, CategoryService>();
            
            services.AddSingleton<ITransactionService, TransactionService>();

            return services.BuildServiceProvider();
        }
    }

}
