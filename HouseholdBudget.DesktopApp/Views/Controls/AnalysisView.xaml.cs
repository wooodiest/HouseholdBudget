using HouseholdBudget.DesktopApp.ViewModels;
using System.Windows.Controls;


namespace HouseholdBudget.DesktopApp.Views.Controls
{
    /// <summary>
    /// Interaction logic for AnalysisView.xaml
    /// </summary>
    public partial class AnalysisView : UserControl
    {
        public AnalysisView()
        {
            InitializeComponent();

            Loaded += AnalysisView_Loaded;
        }

        private void AnalysisView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is BudgetAnalysisViewModel vm)
            {
                vm.TrendPlot = TrendPlot;
                vm.PiePlot = PiePlot;
                _ = vm.LoadAsync();
            }
        }
    }
}
