using HouseholdBudget.Core.Models;

namespace HouseholdBudget.Core.Helpers
{
    public class TransactionFilter
    {
        public List<Guid>? CategoryIds { get; set; }

        public DateTime? Date { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string? DescriptionKeyword { get; set; }

        public decimal? MinAmount { get; set; }

        public decimal? MaxAmount { get; set; }

        public CategoryType? CategoryType { get; set; }

        public bool? IsRecurring { get; set; }
    }
}
