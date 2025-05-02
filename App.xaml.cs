using HouseholdBudget.Data;
using HouseholdBudget.Services;
using System.Configuration;
using System.Data;
using System.Windows;

namespace HouseholdBudget
{
    public partial class App : Application
    {
        public static IDatabaseManager DatabaseManager { get; private set; } = null!;

        public static TransactionService TransactionService { get; private set; } = null!;

        public static CategoryService CategoryService { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            DatabaseManager = new DatabaseManager("householdBudget.db");
            CategoryService = new CategoryService(DatabaseManager);
            TransactionService = new TransactionService(DatabaseManager, CategoryService);

            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }

}
