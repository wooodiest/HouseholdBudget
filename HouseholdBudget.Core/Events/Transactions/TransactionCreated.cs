using HouseholdBudget.Core.Models;

namespace HouseholdBudget.Core.Events.Transactions
{
    /// <summary>
    /// Event raised when a new transaction is successfully created.
    /// </summary>
    public record TransactionCreated(Transaction Transaction) : ITransactionEvent;
}
