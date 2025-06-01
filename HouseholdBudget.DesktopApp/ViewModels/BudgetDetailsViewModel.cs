using HouseholdBudget.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HouseholdBudget.DesktopApp.ViewModels
{
    public class BudgetDetailsViewModel : INotifyPropertyChanged
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public void Load(BudgetPlan plan)
        {
            Name = plan.Name;
            Description = plan.Description ?? "";
            StartDate = plan.StartDate;
            EndDate = plan.EndDate;
            OnPropertyChanged(null);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
