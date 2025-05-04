using HouseholdBudget.Core.Services;
using HouseholdBudget.Desktop.Views;
using System.Windows;
using System.Windows.Controls;

namespace HouseholdBudget.Desktop
{
    public partial class MainWindow : Window
    {
        private readonly ITransactionService _transactionService;

        public MainWindow(IServiceProvider provider)
        {
            InitializeComponent();

            if (Content is Grid grid)
            {
                var view = grid.Children.OfType<TransactionView>().FirstOrDefault();
                view?.Initialize(provider);
            }
        }
    }
}