namespace HouseholdBudget.Core.Events.Transactions
{
    /// <summary>
    /// Event raised when an existing transaction is deleted.
    /// </summary>
    /// <param name="transactionId">The identifier of the deleted transaction.</param>
    public record TransactionDeleted(Guid transactionId) : ITransactionEvent;
}
