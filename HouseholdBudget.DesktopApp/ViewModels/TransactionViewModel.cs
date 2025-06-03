using HouseholdBudget.Core.Models;

namespace HouseholdBudget.DesktopApp.ViewModels
{
    public class TransactionViewModel
    {
        public Transaction Model { get; }

        public DateTime Date => Model.Date;
        public string Description => Model.Description;
        public decimal Amount => Model.Amount;
        public string CurrencyCode => Model.CurrencyCode;
        public TransactionType Type => Model.Type;
        public string CategoryName { get; }

        public TransactionViewModel(Transaction transaction, string categoryName)
        {
            Model = transaction;
            CategoryName = categoryName;
        }
    }
}
