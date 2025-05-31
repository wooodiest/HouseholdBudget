using HouseholdBudget.Core.Models;
using HouseholdBudget.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HouseholdBudget.DesktopApp.Views
{
    /// <summary>
    /// Interaction logic for AddCategoryWindow.xaml
    /// </summary>
    public partial class AddCategoryWindow : Window
    {
        private readonly ICategoryService _categoryService;
        public string? CategoryName { get; private set; }

        public AddCategoryWindow(ICategoryService categoryService)
        {
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
            InitializeComponent();
        }

        private async void Add_Click(object sender, RoutedEventArgs e)
        {
            string name = CategoryNameBox.Text.Trim();

            var erroors = Category.ValidateName(name);
            if (erroors.Any())
            {
                MessageBox.Show(string.Join(Environment.NewLine, erroors), "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            else if (await _categoryService.GetCategoryByNameAsync(name) != null)
            {
                MessageBox.Show("Category with that name already exists.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            CategoryName = CategoryNameBox.Text.Trim();
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
