using HouseholdBudget.Core.Data;
using HouseholdBudget.Core.Events.Transactions;
using HouseholdBudget.Core.Models;
using HouseholdBudget.Core.Services.Interfaces;
using HouseholdBudget.Core.UserData;

namespace HouseholdBudget.Core.Services.Local
{
    /// <summary>
    /// Local implementation of the <see cref="ITransactionService"/> interface. This class manages the lifecycle of
    /// financial transactions associated with the currently authenticated user, including creation, update,
    /// retrieval, and deletion. All transactions are cached in memory to reduce database access and are sorted
    /// by date for optimized filtering and chronological analysis. Domain events are published after each
    /// modification to facilitate system-wide reactions such as logging or reporting.
    /// </summary>
    public class LocalTransactionService : ITransactionService
    {
        private readonly IBudgetRepository _repository;
        private readonly IUserSessionService _userSession;
        private readonly ITransactionEventPublisher _eventPublisher;

        /// <summary>
        /// In-memory cache of the user's transactions, sorted by date for performance in filtering.
        /// The cache is invalidated and rebuilt after significant changes such as create, delete, or updates.
        /// </summary>
        private List<Transaction>? _cachedTransactions;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalTransactionService"/> class.
        /// </summary>
        /// <param name="repository">The repository for data access operations.</param>
        /// <param name="userSession">Provides context about the currently authenticated user.</param>
        /// <param name="eventPublisher">Handles publication of domain events after transaction changes.</param>
        public LocalTransactionService(
            IBudgetRepository repository,
            IUserSessionService userSession,
            ITransactionEventPublisher eventPublisher,
            ICategoryService categoryService)
        {
            _repository = repository;
            _userSession = userSession;
            _eventPublisher = eventPublisher;
        }

        /// <inheritdoc />
        public async Task<Transaction> CreateAsync(
            Guid categoryId,
            decimal amount,
            Currency currency,
            TransactionType type,
            string? description = null,
            IEnumerable<string>? tags = null,
            DateTime? date = null)
        {
            var user = EnsureAuthenticated();
            var transaction = Transaction.Create(user.Id, categoryId, amount, currency, type, description, tags, date);

            await _repository.AddTransactionAsync(transaction);
            await _repository.SaveChangesAsync();

            _cachedTransactions ??= new();
            _cachedTransactions.Add(transaction);
            SortCache();

            await _eventPublisher.PublishAsync(new TransactionCreated(transaction));
            return transaction;
        }

        /// <inheritdoc />
        public async Task DeleteAsync(Guid id)
        {
            var transaction = await GetRequiredTransactionAsync(id);

            await _eventPublisher.PublishAsync(new TransactionDeleted(transaction));
            await _repository.RemoveTransactionAsync(transaction);
            await _repository.SaveChangesAsync();

            _cachedTransactions?.RemoveAll(t => t.Id == id);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Transaction>> GetAsync(TransactionFilter? filter = null)
        {
            var transactions = await GetUserTransactionsAsync();

            if (filter == null)
                return transactions;

            IEnumerable<Transaction> filtered = transactions;

            if (filter.CategoryIds != null)
                filtered = filtered.Where(t => filter.CategoryIds.Contains(t.CategoryId));

            if (filter.Date != null)
                filtered = filtered.Where(t => t.Date.Date == filter.Date.Value.Date);

            if (filter.StartDate != null)
                filtered = filtered.Where(t => t.Date.Date >= filter.StartDate.Value.Date);

            if (filter.EndDate != null)
                filtered = filtered.Where(t => t.Date.Date <= filter.EndDate.Value.Date);

            if (!string.IsNullOrWhiteSpace(filter.DescriptionKeyword))
                filtered = filtered.Where(t => t.Description != null &&
                    t.Description.Contains(filter.DescriptionKeyword, StringComparison.OrdinalIgnoreCase));

            if (filter.MinAmount != null)
                filtered = filtered.Where(t => t.Amount >= filter.MinAmount.Value);

            if (filter.MaxAmount != null)
                filtered = filtered.Where(t => t.Amount <= filter.MaxAmount.Value);

            if (filter.Currency != null)
                filtered = filtered.Where(t => t.Currency.Code == filter.Currency.Code);

            return filtered;
        }

        /// <inheritdoc />
        public async Task<Transaction?> GetByIdAsync(Guid id)
        {
            var transactions = await GetUserTransactionsAsync();
            return transactions.FirstOrDefault(t => t.Id == id);
        }

        /// <inheritdoc />
        public async Task UpdateAmountAsync(Guid id, decimal newAmount)
        {
            var transaction = await GetRequiredTransactionAsync(id);
            transaction.UpdateAmount(newAmount);
            await PersistUpdateAsync(transaction);
        }

        /// <inheritdoc />
        public async Task UpdateCategoryAsync(Guid id, Guid newCategoryId)
        {
            var transaction = await GetRequiredTransactionAsync(id);
            transaction.ChangeCategory(newCategoryId);
            await PersistUpdateAsync(transaction);
        }

        /// <inheritdoc />
        public async Task UpdateCurrencyAsync(Guid id, Currency newCurrency)
        {
            var transaction = await GetRequiredTransactionAsync(id);
            transaction.ChangeCurrency(newCurrency);
            await PersistUpdateAsync(transaction);
        }

        /// <inheritdoc />
        public async Task UpdateDateAsync(Guid id, DateTime newDate)
        {
            var transaction = await GetRequiredTransactionAsync(id);
            transaction.UpdateDate(newDate);
            SortCache();
            await PersistUpdateAsync(transaction);
        }

        /// <inheritdoc />
        public async Task UpdateDescriptionAsync(Guid id, string newDescription)
        {
            var transaction = await GetRequiredTransactionAsync(id);
            transaction.UpdateDescription(newDescription);
            await PersistUpdateAsync(transaction);
        }

        /// <inheritdoc />
        public async Task UpdateTagsAsync(Guid id, IEnumerable<string>? newTags)
        {
            var transaction = await GetRequiredTransactionAsync(id);
            transaction.UpdateTags(newTags);
            await PersistUpdateAsync(transaction);
        }

        /// <inheritdoc />
        public async Task UpdateTypeAsync(Guid id, TransactionType newType)
        {
            var transaction = await GetRequiredTransactionAsync(id);
            transaction.UpdateType(newType);
            await PersistUpdateAsync(transaction);
        }

        /// <summary>
        /// Retrieves transactions from cache or loads them from the repository if cache is null.
        /// Ensures that the result is sorted chronologically.
        /// </summary>
        /// <returns>A list of transactions associated with the current user.</returns>
        private async Task<IEnumerable<Transaction>> GetUserTransactionsAsync()
        {
            if (_cachedTransactions == null)
            {
                var user = EnsureAuthenticated();
                var loaded = await _repository.GetTransactionsByUserAsync(user.Id);
                _cachedTransactions = loaded.OrderBy(t => t.Date).ToList();
            }

            return _cachedTransactions;
        }

        /// <summary>
        /// Persists updates to a transaction, saves changes in the repository, and raises a domain event.
        /// </summary>
        /// <param name="transaction">The updated transaction to persist.</param>
        private async Task PersistUpdateAsync(Transaction transaction)
        {
            await _repository.UpdateTransactionAsync(transaction);
            await _repository.SaveChangesAsync();
            await _eventPublisher.PublishAsync(new TransactionUpdated(transaction));
        }

        /// <summary>
        /// Retrieves a transaction by ID or throws if not found.
        /// </summary>
        /// <param name="id">The transaction ID.</param>
        /// <returns>The matching transaction.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the transaction is not found.</exception>
        private async Task<Transaction> GetRequiredTransactionAsync(Guid id)
        {
            var transaction = await GetByIdAsync(id);
            return transaction ?? throw new InvalidOperationException("Transaction not found.");
        }

        /// <summary>
        /// Retrieves the currently authenticated user or throws an exception if not logged in.
        /// </summary>
        /// <returns>The authenticated user.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no user is authenticated.</exception>
        private User EnsureAuthenticated() =>
            _userSession.GetUser() ?? throw new InvalidOperationException("User is not authenticated.");

        /// <summary>
        /// Sorts the in-memory cache of transactions in ascending order by transaction date.
        /// </summary>
        private void SortCache()
        {
            _cachedTransactions?.Sort((a, b) => a.Date.CompareTo(b.Date));
        }
    }
}
