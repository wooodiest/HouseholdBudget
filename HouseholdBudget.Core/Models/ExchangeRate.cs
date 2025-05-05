namespace HouseholdBudget.Core.Models
{
    public class ExchangeRate
    {
        public required string FromCurrencyCode { get; set; }

        public required string ToCurrencyCode { get; set; }

        public required decimal Rate { get; set; }

        public DateTime RetrievedAt { get; set; } = DateTime.UtcNow;
    }
}
