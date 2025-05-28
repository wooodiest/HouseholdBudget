using HouseholdBudget.Core.Data;
using HouseholdBudget.Core.Events.Transactions;
using HouseholdBudget.Core.Services.Interfaces;
using HouseholdBudget.Core.Services.Local;
using HouseholdBudget.Core.Services.Shared;
using HouseholdBudget.Core.UserData;
using HouseholdBudget.DesktopApp.Infrastructure;
using HouseholdBudget.DesktopApp.ViewModels;
using HouseholdBudget.DesktopApp.Views;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;
using System;

namespace HouseholdBudget.DesktopApp
{
    /// <summary>
    /// Responsible for bootstrapping the application and registering all services, 
    /// view models, windows, and infrastructure components in the dependency injection container.
    /// </summary>
    class AppBootstrapper
    {
        /// <summary>
        /// Configures and builds the application's service provider.
        /// Registers all domain, infrastructure, and UI-level services.
        /// </summary>
        /// <returns>The configured <see cref="IServiceProvider"/> instance for the application.</returns>
        public static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // ---------- Database Configuration ----------
            // Configure the EF Core database context with SQLite persistence.
            services.AddDbContext<BudgetDbContext>(options =>
                options.UseSqlite("Data Source=household.db"));
            services.AddScoped<IBudgetRepository, BudgetRepository>();

            // ---------- User Authentication & Identity ----------
            // Manage user authentication and identity services.
            services.AddScoped<IUserAuthenticator, UserAuthenticator>();
            services.AddScoped<IUserSessionService, UserSessionService>();
            services.AddScoped<IUserContext, UserContext>();
            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

            // ---------- Exchange Rate Services ----------
            // Static/local exchange rate service for converting currencies.
            services.AddSingleton<IExchangeRateProvider, LocalExchangeRateProvider>();
            services.AddSingleton<IExchangeRateService, ExchangeRateService>();

            // ---------- Domain Services ----------
            // Register domain-specific business services.
            services.AddScoped<ICategoryService, LocalCategoryService>();
            services.AddScoped<ITransactionService, LocalTransactionService>();
            services.AddScoped<IBudgetAnalysisService, LocalBudgetAnalysisService>();

            // ---------- Domain Event Dispatcher ----------
            // Register the dispatcher and dynamic event handlers for transaction events.
            services.AddScoped<ITransactionEventPublisher, TransactionEventDispatcher>();
            services.Scan(scan => scan
                .FromAssemblyOf<ITransactionEventHandler>()
                .AddClasses(c => c.AssignableTo<ITransactionEventHandler>())
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            // ---------- UI Layer ----------
            // Register view models and window managers for WPF.
            services.AddSingleton<IWindowManager, WindowManager>();

            services.AddTransient<MainWindow>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<LoginWindow>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<RegisterViewModel>();

            // Custom factory for the combined AuthViewModel with login/register tabs.
            services.AddTransient<AuthViewModel>(provider =>
            {
                var loginVM = provider.GetRequiredService<LoginViewModel>();
                var registerVM = provider.GetRequiredService<RegisterViewModel>();
                return new AuthViewModel(loginVM, registerVM);
            });

            // ---------- Database Initialization ----------
            // Ensure the database is created and migrations are applied on startup.
            var serviceProvider = services.BuildServiceProvider();

            using (var scope = serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BudgetDbContext>();
                db.Database.Migrate();
            }

            return serviceProvider;
        }
    }
}
