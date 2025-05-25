using HouseholdBudget.Core.Data;
using HouseholdBudget.Core.Services;
using HouseholdBudget.Core.UserData;
using HouseholdBudget.DesktopApp.Infrastructure;
using HouseholdBudget.DesktopApp.ViewModels;
using HouseholdBudget.DesktopApp.Views;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace HouseholdBudget.DesktopApp
{
    class AppBootstrapper
    {
        public static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Database
            services.AddDbContext<BudgetDbContext>(options =>
                options.UseSqlite("Data Source=household.db"));
            services.AddScoped<IBudgetRepository, BudgetRepository>();

            // User management
            services.AddScoped<IUserAuthenticator, UserAuthenticator>();
            services.AddScoped<IUserSessionService, UserSessionService>();
            services.AddScoped<IUserContext, UserContext>();
            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

            // Exchange rate services
            services.AddSingleton<IExchangeRateProvider, LocalExchangeRateProvider>();
            services.AddSingleton<IExchangeRateService, ExchangeRateService>();

            // Windows and view models
            services.AddSingleton<IWindowManager, WindowManager>();

            services.AddTransient<MainWindow>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<LoginWindow>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<RegisterViewModel>();
            services.AddTransient<AuthViewModel>(provider => {
                var loginVM = provider.GetRequiredService<LoginViewModel>();
                var registerVM = provider.GetRequiredService<RegisterViewModel>();
                return new AuthViewModel(loginVM, registerVM);
            });

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
