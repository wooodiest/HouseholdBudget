namespace HouseholdBudget.Core.Events.Transactions
{
    /// <summary>
    /// Default implementation of <see cref="ITransactionEventPublisher"/> that forwards events to registered handlers.
    /// </summary>
    public class TransactionEventDispatcher : ITransactionEventPublisher
    {
        private readonly IEnumerable<ITransactionEventHandler> _handlers;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionEventDispatcher"/> class.
        /// </summary>
        /// <param name="handlers">All event handlers registered for transaction domain events.</param>
        public TransactionEventDispatcher(IEnumerable<ITransactionEventHandler> handlers)
        {
            _handlers = handlers;
        }

        /// <inheritdoc />
        public async Task PublishAsync(ITransactionEvent domainEvent)
        {
            foreach (var handler in _handlers)
            {
                await handler.HandleAsync(domainEvent);
            }
        }
    }
}
