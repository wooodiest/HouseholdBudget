using HouseholdBudget.Core.Services.Interfaces;
using HouseholdBudget.DesktopApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;


namespace HouseholdBudget.DesktopApp.Views.Controls
{
    /// <summary>
    /// Interaction logic for AnalysisView.xaml
    /// </summary>
    public partial class AnalysisView : UserControl
    {
        private readonly BudgetAnalysisViewModel _vm;

        public AnalysisView(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            var analysisService = serviceProvider.GetRequiredService<IBudgetAnalysisService>();
            var categoryService = serviceProvider.GetRequiredService<ICategoryService>();
            _vm = new BudgetAnalysisViewModel(analysisService, categoryService);
            this.DataContext = _vm;

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _vm.TrendPlot = TrendPlot;
            _vm.PiePlot = PiePlot;
            _vm.CustomTrendPlot = CustomTrendPlot;

            _ = _vm.LoadMonthlyAsync();
            _ = _vm.LoadCustomRangeAsync();
        }
    }
}
