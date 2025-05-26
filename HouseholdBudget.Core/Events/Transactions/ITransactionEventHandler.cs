namespace HouseholdBudget.Core.Events.Transactions;

/// <summary>
/// Defines a handler for transaction-related domain events.
/// </summary>
public interface ITransactionEventHandler
{
    /// <summary>
    /// Handles the specified transaction event.
    /// </summary>
    /// <param name="domainEvent">The event to handle.</param>
    Task HandleAsync(ITransactionEvent domainEvent);
}
