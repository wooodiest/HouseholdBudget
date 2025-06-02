using HouseholdBudget.Core.Models;
using HouseholdBudget.Core.Services.Interfaces;
using HouseholdBudget.Core.UserData;
using HouseholdBudget.DesktopApp.Commands;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace HouseholdBudget.DesktopApp.ViewModels
{
    public class AddBudgetViewModel : INotifyPropertyChanged
    {
        private readonly BudgetPlan? _existing;

        public string HeaderText => _existing != null ? "Update Budget" : "Add Budget";
        public string ButtonText => _existing != null ? "Update" : "Add";

        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public DateTime? StartDate { get; set; } = DateTime.Today;
        public DateTime? EndDate { get; set; } = DateTime.Today.AddDays(30);

        public AddBudgetViewModel(BudgetPlan? existing)
        {
            _existing      = existing;

            if (_existing != null)
            {
                Name        = _existing.Name;
                Description = _existing.Description;
                StartDate   = _existing.StartDate;
                EndDate     = _existing.EndDate;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
