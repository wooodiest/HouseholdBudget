﻿using HouseholdBudget.Core.Models;
using HouseholdBudget.Core.Services.Interfaces;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using HouseholdBudget.DesktopApp.Commands;
using ScottPlot.Plottables;
using ScottPlot.WPF;
using System.Drawing;

namespace HouseholdBudget.DesktopApp.ViewModels
{
    public class BudgetAnalysisViewModel : INotifyPropertyChanged
    {
        private readonly IBudgetAnalysisService _analysisService;
        private readonly ICategoryService _categoryService;

        public WpfPlot? TrendPlot { get; set; }
        public WpfPlot? ExpensePiePlot { get; set; }
        public WpfPlot? IncomePiePlot { get; set; }

        public WpfPlot? CustomTrendPlot { get; set; }
        public WpfPlot? CustomExpensePiePlot { get; set; }
        public WpfPlot? CustomIncomePiePlot { get; set; }

        public ObservableCollection<int> Years { get; } = new(Enumerable.Range(2020, 10));
        private static readonly CultureInfo _englishCulture = new("en-US");

        public ObservableCollection<string> Months { get; } =
            new(_englishCulture.DateTimeFormat.MonthNames
                .Where(m => !string.IsNullOrEmpty(m))
                .ToList());

        public string SelectedMonth { get; set; } = DateTime.Now.ToString("MMMM", _englishCulture);

        public int SelectedYear { get; set; } = DateTime.Now.Year;

        public DateTime? CustomStartDate { get; set; } = DateTime.Now.AddMonths(-1);
        public DateTime? CustomEndDate { get; set; } = DateTime.Now.AddMonths(1);

        public ICommand RefreshCommand { get; }
        public ICommand ApplyCustomDateRangeCommand { get; }

        public BudgetAnalysisViewModel(IBudgetAnalysisService analysisService, ICategoryService categoryService)
        {
            _analysisService = analysisService;
            _categoryService = categoryService;

            RefreshCommand = new BasicRelayCommand(async () => await LoadMonthlyAsync());
            ApplyCustomDateRangeCommand = new BasicRelayCommand(async () => await LoadCustomRangeAsync());
        }

        public async Task LoadMonthlyAsync()
        {
            if (!DateTime.TryParseExact(SelectedMonth, "MMMM", _englishCulture, DateTimeStyles.None, out var dt))
                return;

            int month = dt.Month;
            var summary = await _analysisService.GetMonthlySummaryAsync(SelectedYear, month);

            DrawTrend(TrendPlot!, summary, custom: false);
            DrawPie(ExpensePiePlot!, summary, isIncome: false);
            DrawPie(IncomePiePlot!, summary, isIncome: true);
        }

        public async Task LoadCustomRangeAsync()
        {
            if (CustomStartDate == null || CustomEndDate == null)
                return;

            var start = CustomStartDate.Value.Date;
            var end   = CustomEndDate.Value.Date;

            var trend = await _analysisService.GetDailyTrendAsync(start, end);
            var currency = trend.FirstOrDefault()?.CurrencyCode;

            var current = new DateTime(start.Year, start.Month, 1);
            var grouped = new Dictionary<Guid, (decimal Income, decimal Expenses)>();

            while (current <= end)
            {
                var monthStart = current;
                var monthEnd = new DateTime(current.Year, current.Month, DateTime.DaysInMonth(current.Year, current.Month));

                var sliceStart = monthStart < start ? start : monthStart;
                var sliceEnd = monthEnd > end ? end : monthEnd;

                var monthlyCategories = await _analysisService.GetCategoryBreakdownAsync(sliceStart, sliceEnd);

                foreach (var cat in monthlyCategories)
                {
                    if (!grouped.TryGetValue(cat.CategoryId, out var totals))
                        totals = (0, 0);

                    totals.Income   += cat.TotalIncome;
                    totals.Expenses += cat.TotalExpenses;
                    grouped[cat.CategoryId] = totals;
                }

                current = current.AddMonths(1);
            }

            var mergedCategories = grouped.Select(kvp =>
                new CategoryBudgetBreakdown(
                    kvp.Key,
                    kvp.Value.Income,
                    kvp.Value.Expenses,
                    currency!
                )).ToList();

            var summary = new MonthlyBudgetSummary {
                CurrencyCode = currency,
                DailyTrend = trend.ToList(),
                Categories = mergedCategories
            };

            DrawTrend(CustomTrendPlot!, summary, custom: true);
            DrawPie(CustomExpensePiePlot!, summary, isIncome: false);
            DrawPie(CustomIncomePiePlot!, summary, isIncome: true);
        }

        private void DrawTrend(WpfPlot plot, MonthlyBudgetSummary summary, bool custom)
        {
            if (plot is null)
                return;

            var plt = plot.Plot;
            plt.Clear();

            var data = summary.DailyTrend;
            if (data.Count == 0)
            {
                plt.Title("No data");
                plot.Refresh();
                return;
            }

            var dates   = data.Select(p => p.Date.ToOADate()).ToArray();
            var income  = data.Select(p => (double)p.TotalIncome).ToArray();
            var expense = data.Select(p => (double)p.TotalExpenses).ToArray();

            string currencySymbol = summary.CurrencyCode ?? "unknow";

            var incomePlot = plt.Add.Scatter(dates, income);
            incomePlot.Label = $"Income ({currencySymbol})";

            var expensePlot = plt.Add.Scatter(dates, expense);
            expensePlot.Label = $"Expenses ({currencySymbol})";

            plt.Title(custom ? "Custom Trend" : $"Monthly Trend ({SelectedMonth} {SelectedYear})");
            plt.Legend.IsVisible = true;
            plt.Legend.Alignment = Alignment.UpperRight;
            plt.Axes.DateTimeTicksBottom();
            plt.Axes.AutoScale();

            plt.Axes.Bottom.TickLabelStyle.FontSize = 16;
            plt.Axes.Left.TickLabelStyle.FontSize = 16;

            ApplyTheme(plt);
            plot.Refresh();
        }

        private async void DrawPie(WpfPlot plot, MonthlyBudgetSummary summary, bool isIncome)
        {
            if (plot is null)
                return;

            var plt = plot.Plot;
            plt.Clear();
            ApplyTheme(plt);
            plt.Axes.Margins(0.0, 0.0);
            plt.Axes.Bottom.TickLabelStyle.IsVisible = false;
            plt.Axes.Bottom.MajorTickStyle.Length = 0;
            plt.Axes.Bottom.MinorTickStyle.Length = 0;
            plt.Axes.Left.TickLabelStyle.IsVisible = false;
            plt.Axes.Left.MajorTickStyle.Length = 0;
            plt.Axes.Left.MinorTickStyle.Length = 0;
            plt.Axes.SetLimits(-1.4, 1.4, -1.4, 1.4);
            plt.Axes.AutoScale();
            plt.Layout.Frameless();

            var typeName = isIncome ? "Income" : "Expenses";
            var color    = isIncome ? "#44BB44" : "#BB4444";

            var data = summary.Categories
                .Where(c => isIncome ? c.TotalIncome > 0 : c.TotalExpenses > 0)
                .ToList();

            if (data.Count == 0)
            {
                plt.Title("No category data");
                plot.Refresh();
                return;
            }

            var categoryTasks   = data.Select(c => _categoryService.GetCategoryByIdAsync(c.CategoryId)).ToArray();
            var categoryObjects = await Task.WhenAll(categoryTasks);

            double[] values = data
                    .Select(c => (double)(isIncome ? c.TotalIncome : c.TotalExpenses))
                    .ToArray();
            string[] labels = categoryObjects.Select(c => c?.Name ?? "Unknown").ToArray();

            var pie = plt.Add.Pie(values);
            for (int i = 0; i < pie.Slices.Count; i++)
            {
                pie.Slices[i].Label = labels[i];
                pie.Slices[i].LabelFontColor = ScottPlot.Color.FromHex("#DDDDDD");
                pie.Slices[i].LabelFontSize = 16;
            }

            plt.Legend.IsVisible = false;

            string currencySymbol = summary.CurrencyCode ?? "Unknown";
            double total = data.Sum(c => (double)(isIncome ? c.TotalIncome : c.TotalExpenses));
            var annotation = plt.Add.Annotation($"Total: {total:N2} {currencySymbol}");
            annotation.LabelBackgroundColor = ScottPlot.Color.FromHex("#DDDDDD");
            annotation.LabelFontSize = 16;

            ApplyTheme(plt);
            plot.Refresh();
        }

        public void ApplyTheme(Plot plot)
        {
            var background = ScottPlot.Color.FromHex("#2C2C3E"); 
            var dataBackground = ScottPlot.Color.FromHex("#1E1E2F"); 
            var axisColor = ScottPlot.Color.FromHex("#DDDDDD");
            var gridColor = ScottPlot.Color.FromHex("#444444"); 
            var legendBg = ScottPlot.Color.FromHex("#444466"); 

            plot.FigureBackground.Color = background;
            plot.DataBackground.Color = dataBackground;

            plot.Axes.Color(axisColor);
            plot.Grid.MajorLineColor = gridColor;

            plot.Axes.Color(axisColor);

            plot.Legend.BackgroundColor = legendBg;
            plot.Legend.FontColor = axisColor;
            plot.Legend.OutlineColor = axisColor;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
