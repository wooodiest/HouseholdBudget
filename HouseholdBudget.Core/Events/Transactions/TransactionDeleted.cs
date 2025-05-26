using HouseholdBudget.Core.Models;

namespace HouseholdBudget.Core.Events.Transactions
{
    /// <summary>
    /// Event raised when an existing transaction is deleted.
    /// </summary>
    /// <param name="Transaction">The transaction object being deleted.</param>
    public record TransactionDeleted(Transaction Transaction) : ITransactionEvent;
}
