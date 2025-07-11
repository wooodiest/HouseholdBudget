﻿using System.ComponentModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using HouseholdBudget.Core.Services.Interfaces;
using HouseholdBudget.Core.UserData;
using HouseholdBudget.DesktopApp.Infrastructure;
using HouseholdBudget.DesktopApp.ViewModels;
using HouseholdBudget.DesktopApp.Views;
using Microsoft.Extensions.DependencyInjection;

namespace HouseholdBudget.DesktopApp
{
    public partial class MainWindow : Window
    {
        private readonly IWindowManager _windowManager;
        private readonly IBudgetPlanService _budgetService;
        private readonly IUserSessionService _userSessionService;
        private readonly IViewRouter _viewRouter;
        private readonly MainViewModel _viewModel;

        public MainWindow(IWindowManager windowManager, IViewRouter iViewRouter,
            IBudgetPlanService budgetPlanService, IUserSessionService userSessionService, MainViewModel viewModel)
        {
            InitializeComponent();
            _windowManager = windowManager;
            _viewRouter    = iViewRouter;
            _budgetService = budgetPlanService;
            _userSessionService = userSessionService;
            _viewModel     = viewModel;

            DataContext = _viewModel;
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.UpdateUserDisplay();
            _viewModel.Logout();
            _windowManager.ShowLoginWindow();
            this.Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void TransactionsButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ShowTransactions();
        }

        private void AnalysisButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ShowAnalysis();
        }

        private void ReceiptButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ShowReceipt();
        }

        private void AddBudget_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddBudgetWindow(_budgetService, _userSessionService) {
                Owner = Application.Current.MainWindow
            };

            if (window.ShowDialog() == true && window.Result != null)
            {
                _viewModel.Budgets.Add(window.Result);
            }
        }

        private async void RemoveBudget_Click(object sender, RoutedEventArgs e)
        {
            await _viewModel.DeleteBudget();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void Resize_Left(object sender, DragDeltaEventArgs e)
        {
            double newWidth = Width - e.HorizontalChange;
            if (newWidth >= MinWidth)
            {
                Left += e.HorizontalChange;
                Width = newWidth;
            }
        }

        private void Resize_Right(object sender, DragDeltaEventArgs e)
        {
            double newWidth = Width + e.HorizontalChange;
            if (newWidth >= MinWidth)
                Width = newWidth;
        }

        private void Resize_Top(object sender, DragDeltaEventArgs e)
        {
            double newHeight = Height - e.VerticalChange;
            if (newHeight >= MinHeight)
            {
                Top += e.VerticalChange;
                Height = newHeight;
            }
        }

        private void Resize_Bottom(object sender, DragDeltaEventArgs e)
        {
            double newHeight = Height + e.VerticalChange;
            if (newHeight >= MinHeight)
                Height = newHeight;
        }

        private void Resize_TopLeft(object sender, DragDeltaEventArgs e)
        {
            Resize_Top(sender, e);
            Resize_Left(sender, e);
        }

        private void Resize_TopRight(object sender, DragDeltaEventArgs e)
        {
            Resize_Top(sender, e);
            Resize_Right(sender, e);
        }

        private void Resize_BottomLeft(object sender, DragDeltaEventArgs e)
        {
            Resize_Bottom(sender, e);
            Resize_Left(sender, e);
        }

        private void Resize_BottomRight(object sender, DragDeltaEventArgs e)
        {
            Resize_Bottom(sender, e);
            Resize_Right(sender, e);
        }

    }

}
