using HouseholdBudget.Core.Data;
using HouseholdBudget.Core.Services;
using System.Windows;

namespace HouseholdBudget.Desktop
{
    public partial class App : Application
    {
        public static IDatabaseManager DatabaseManager { get; private set; } = null!;

        public static ITransactionService TransactionService { get; private set; } = null!;

        public static ICategoryService CategoryService { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            DatabaseManager = new LocalDatabaseManager("householdBudget.db");
            CategoryService = new CategoryService(DatabaseManager);
            TransactionService = new TransactionService(DatabaseManager, CategoryService);

            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }

}
