using HouseholdBudget.Core.Data;
using HouseholdBudget.Core.Events.Transactions;
using HouseholdBudget.Core.Models;
using HouseholdBudget.Core.Services.Interfaces;
using HouseholdBudget.Core.Services.Local;
using HouseholdBudget.Core.Services.Shared;
using HouseholdBudget.Core.UserData;
using HouseholdBudget.DesktopApp.Infrastructure;
using HouseholdBudget.DesktopApp.ViewModels;
using HouseholdBudget.DesktopApp.Views;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;
using System;
using System.Windows;
using System.Xml.Linq;

namespace HouseholdBudget.DesktopApp
{
    public class LazyResolver<T> : Lazy<T>
    {
        public LazyResolver(IServiceProvider provider)
            : base(() => provider.GetRequiredService<T>())
        {
        }
    }
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

            // ---------- Core Services ----------
            services.AddTransient(typeof(Lazy<>), typeof(LazyResolver<>));

            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // ---------- Database Configuration ----------
            // Configure the EF Core database context with SQLite persistence.
            services.AddDbContext<BudgetDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("AzureSql")));

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
            services.AddScoped<IBudgetPlanService, LocalBudgetPlanService>();
            services.AddScoped<IBudgetExecutionService, LocalBudgetExecutionService>();

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
            services.AddSingleton<IViewRouter, ViewRouter>();

            services.AddTransient<MainWindow>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<LoginWindow>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<RegisterViewModel>();
            services.AddTransient<TransactionsViewModel>();
            services.AddTransient<BudgetAnalysisViewModel>();
            services.AddTransient<BudgetDetailsViewModel>();
            services.AddTransient<AddBudgetViewModel>();
            services.AddTransient<AddBudgetWindow>();
            services.AddTransient<BudgetDetailsViewModel>();

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
                SeedTestData(serviceProvider);
            }

            return serviceProvider;
        }

        private static void SeedTestData(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BudgetDbContext>();

            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
            var authenticator  = scope.ServiceProvider.GetRequiredService<IUserAuthenticator>();
            var userContext    = scope.ServiceProvider.GetRequiredService<IUserContext>();

            var categoryService    = scope.ServiceProvider.GetRequiredService<ICategoryService>();
            var transactionService = scope.ServiceProvider.GetRequiredService<ITransactionService>();
            var budgetPlanService  = scope.ServiceProvider.GetRequiredService<IBudgetPlanService>();

            if (!db.Users.Any())
            {
                var user = User.Create(
                    name: "Demo user",
                    email: "demo@example.com",
                    passwordHash: passwordHasher.HashPassword(null!, "DemoPassword123#"),
                    defaultCurrencyCode: "PLN"
                );

                db.Users.Add(user);
                db.SaveChanges();
                userContext.SetUser(user);

                // Add categories
                var categories = new[]
                {
                    "Food",
                    "Rent",
                    "Utilities",
                    "Transportation",
                    "Healthcare",
                    "Entertainment",
                    "Education",
                    "Groceries",
                    "Clothing",
                    "Savings",
                    "Dining Out",
                    "Subscriptions",
                    "Insurance",
                    "Pets",
                    "Gifts",
                    "Travel",
                    "Household Supplies",
                    "Internet",
                    "Phone",
                    "Childcare"
                };

                var createdCategories = new Dictionary<string, Category>();
                foreach (var cat in categories)
                {
                    var created = categoryService.CreateCategoryAsync(cat).GetAwaiter().GetResult();
                    createdCategories[cat] = created;
                }

                // Add some transactions
                var random = new Random();
                var now = DateTime.Today;
                var currency = "PLN";
                var sampleDescriptions = new[]
                {
                    "Weekly groceries", "Dinner with friends", "Bus ticket", "Doctor appointment", "Netflix subscription",
                    "Electricity bill", "Monthly rent", "School books", "Fuel", "Clothes shopping", "Gift for mom",
                    "Insurance fee", "New shoes", "Pharmacy", "Movie night", "Pet food", "Train ride", "Internet bill"
                };

                for (int i = 0; i < 160; i++)
                {
                    var isExpense = random.NextDouble() < 0.9;

                    var category    = isExpense
                                ? createdCategories.Where(c => c.Key != "Savings").Select(c => c.Value).ElementAt(random.Next(createdCategories.Count - 1))
                                : createdCategories["Savings"];

                    decimal amount;
                    if (category.Name is "Rent" or "Savings" or "Insurance")
                        amount = Math.Round((decimal)(random.NextDouble() * 2000 + 500), 2);
                    else
                        amount = Math.Round((decimal)(random.NextDouble() * 500 + 10), 2);

                    var description = sampleDescriptions[random.Next(sampleDescriptions.Length)];
                    var date        = now.AddDays(-random.Next(-100, 100)).AddHours(random.Next(0, 24)).AddMinutes(random.Next(0, 60));
                    var type        = isExpense ? TransactionType.Expense : TransactionType.Income;

                    transactionService.CreateAsync(
                        categoryId: category.Id,
                        amount: amount,
                        currencyCode: currency,
                        type: type,
                        description: description,
                        date: date
                    ).GetAwaiter().GetResult();
                }

                var budgets = new[]
                {
                    new { Name = "March Essentials",     Desc = "Basic expenses for March",         Start = now.AddDays(-60), End = now.AddDays(-30) },
                    new { Name = "Spring Fun",           Desc = "Leisure and outings",              Start = now.AddDays(-45), End = now.AddDays(-15) },
                    new { Name = "April Full Budget",    Desc = "Complete monthly budget",          Start = now.AddDays(-30), End = now.AddDays(0) },
                    new { Name = "May Minimal Plan",     Desc = "Frugal spending approach",         Start = now.AddDays(-15), End = now.AddDays(15) },
                    new { Name = "Vacation Savings",     Desc = "Budgeting for the upcoming trip",  Start = now.AddDays(0),   End = now.AddDays(30) },
                };

                var categoryList = createdCategories.Values.ToList();
                foreach (var b in budgets)
                {
                    var plan = BudgetPlan.Create(user.Id, b.Name, b.Start, b.End, b.Desc);

                    var numberOfCategories = random.Next(3, 8); // od 3 do 7 kategorii
                    var shuffledCategories = categoryList.OrderBy(_ => random.Next()).Take(numberOfCategories);

                    foreach (var category in shuffledCategories)
                    {
                        var allocation = new CategoryBudgetPlan(
                            Guid.NewGuid(),
                            category.Id,
                            incomePlanned: Math.Round((decimal)(random.NextDouble() * 1200 + 100), 2),
                            expensePlanned: Math.Round((decimal)(random.NextDouble() * 1200 + 100), 2),
                            currencyCode: "PLN"
                        );

                        plan.AddCategoryPlan(allocation);
                    }

                    budgetPlanService.CreatePlanAsync(user.Id, plan.Name, plan.StartDate, plan.EndDate, plan.Description, plan.CategoryPlans);
                }

            }
        }

    }
}
