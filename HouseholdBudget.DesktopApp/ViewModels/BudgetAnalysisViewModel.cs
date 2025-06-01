using HouseholdBudget.Core.Services.Interfaces;
using HouseholdBudget.Core.Models;
using ScottPlot;
using ScottPlot.Plottables;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using HouseholdBudget.DesktopApp.Commands;
using System.Threading.Tasks;
using ScottPlot.WPF;

namespace HouseholdBudget.DesktopApp.ViewModels
{
    public class BudgetAnalysisViewModel : INotifyPropertyChanged
    {
        private readonly IBudgetAnalysisService _analysisService;

        public WpfPlot? TrendPlot { get; set; }
        public WpfPlot? PiePlot { get; set; }

        public ObservableCollection<int> Years { get; } = new(Enumerable.Range(2020, 10));
        public ObservableCollection<string> Months { get; } = new(System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.MonthNames.Take(12).ToList());

        public string SelectedMonth { get; set; } = DateTime.Now.ToString("MMMM");
        public int SelectedYear { get; set; } = DateTime.Now.Year;

        public ICommand RefreshCommand { get; }

        public BudgetAnalysisViewModel(IBudgetAnalysisService analysisService)
        {
            _analysisService = analysisService;
            RefreshCommand = new BasicRelayCommand(async () => await LoadAsync());
            _ = LoadAsync();
        }

        public async Task LoadAsync()
        {
            int month = DateTime.ParseExact(SelectedMonth, "MMMM", null).Month;
            var summary = await _analysisService.GetMonthlySummaryAsync(SelectedYear, month);

            DrawTrend(summary);
            DrawPie(summary);
        }

        private void DrawTrend(MonthlyBudgetSummary summary)
        {
            if (TrendPlot is null) return;
            var plt = TrendPlot.Plot;
            plt.Clear();

            var dates = summary.DailyTrend.Select(p => p.Date.ToOADate()).ToArray();
            var incomes = summary.DailyTrend.Select(p => (double)p.TotalIncome).ToArray();
            var expenses = summary.DailyTrend.Select(p => (double)p.TotalExpenses).ToArray();

            var incomePlot = plt.Add.Scatter(dates, incomes);
            incomePlot.Label = "Income";
            var expensePlot = plt.Add.Scatter(dates, expenses);
            expensePlot.Label = "Expenses";

            plt.Axes.DateTimeTicksBottom();
            plt.Title($"Daily Budget Trend ({SelectedMonth} {SelectedYear})");
            plt.XLabel("Date");
            plt.YLabel($"Amount ({summary.Currency.Code})");
            plt.Legend.IsVisible = true;

            TrendPlot.Refresh();
        }

        private void DrawPie(MonthlyBudgetSummary summary)
        {
            if (PiePlot is null) return;

            var plt = PiePlot.Plot;
            plt.Clear();

            var data = summary.Categories.Where(c => c.Amount > 0).ToList();
            double[] values = data.Select(c => (double)c.Amount).ToArray();
            string[] labels = data.Select(c => $"Cat {c.CategoryId}").ToArray(); // tymczasowe

            if (values.Length > 0)
            {
                var pie = plt.Add.Pie(values);

                // Dodajemy legendę z ręcznie przypisanymi nazwami
                for (int i = 0; i < labels.Length; i++)
                {
                    pie.Slices[i].Label = labels[i];
                }

                plt.Legend.IsVisible = true;
                plt.Title("Expenses by Category");
            }
            else
            {
                plt.Title("No data to display");
            }

            PiePlot.Refresh();
        }




        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
