namespace HouseholdBudget.Core.Events.Transactions;

/// <summary>
/// Publishes transaction-related domain events to subscribed handlers.
/// </summary>
public interface ITransactionEventPublisher
{
    /// <summary>
    /// Publishes the specified transaction event.
    /// </summary>
    /// <param name="domainEvent">The event to publish.</param>
    Task PublishAsync(ITransactionEvent domainEvent);
}
