using HouseholdBudget.Core.Models;

namespace HouseholdBudget.Core.Events.Transactions
{
    /// <summary>
    /// Event raised when a transaction is updated.
    /// </summary>
    /// <param name="Transaction">The updated transaction object.</param>
    public record TransactionUpdated(Transaction Transaction) : ITransactionEvent;
}
