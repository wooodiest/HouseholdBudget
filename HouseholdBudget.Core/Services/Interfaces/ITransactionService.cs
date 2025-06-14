﻿using HouseholdBudget.Core.Models;

namespace HouseholdBudget.Core.Services.Interfaces
{
    /// <summary>
    /// Provides high-level operations for managing financial transactions associated with the currently authenticated user.
    /// This service supports creation, retrieval, filtering, and granular updates to individual transaction fields.
    /// </summary>
    public interface ITransactionService
    {
        /// <summary>
        /// Retrieves all transactions that match the provided filtering criteria.
        /// </summary>
        /// <param name="filter">Optional filter to apply to the transaction query.</param>
        /// <returns>A list of matching transactions.</returns>
        Task<IEnumerable<Transaction>> GetAsync(TransactionFilter? filter = null);

        /// <summary>
        /// Retrieves a single transaction by its unique identifier.
        /// </summary>
        /// <param name="id">The ID of the transaction to retrieve.</param>
        /// <returns>The transaction if found, otherwise null.</returns>
        Task<Transaction?> GetByIdAsync(Guid id);

        /// <summary>
        /// Creates and persists a new transaction for the current user.
        /// </summary>
        /// <param name="categoryId">The ID of the category this transaction belongs to.</param>
        /// <param name="amount">The monetary value of the transaction.</param>
        /// <param name="currencyCode">The currency in which the transaction is made.</param>
        /// <param name="type">The type of transaction (income or expense).</param>
        /// <param name="description">Optional text description of the transaction.</param>
        /// <param name="date">Optional date of the transaction. Defaults to the current UTC time.</param>
        /// <returns>The newly created transaction.</returns>
        Task<Transaction> CreateAsync(
            Guid categoryId,
            decimal amount,
            string currencyCode,
            TransactionType type,
            string? description = null,
            DateTime? date = null);

        /// <summary>
        /// Deletes an existing transaction that belongs to the current user.
        /// </summary>
        /// <param name="id">The ID of the transaction to delete.</param>
        Task DeleteAsync(Guid id);

        /// <summary>
        /// Updates the description of an existing transaction.
        /// </summary>
        /// <param name="id">The ID of the transaction to update.</param>
        /// <param name="newDescription">The new description text.</param>
        Task UpdateDescriptionAsync(Guid id, string newDescription);

        /// <summary>
        /// Updates the amount of an existing transaction.
        /// </summary>
        /// <param name="id">The ID of the transaction to update.</param>
        /// <param name="newAmount">The new amount value.</param>
        Task UpdateAmountAsync(Guid id, decimal newAmount);

        /// <summary>
        /// Updates the date of an existing transaction.
        /// </summary>
        /// <param name="id">The ID of the transaction to update.</param>
        /// <param name="newDate">The new UTC date.</param>
        Task UpdateDateAsync(Guid id, DateTime newDate);

        /// <summary>
        /// Updates the category of an existing transaction.
        /// </summary>
        /// <param name="id">The ID of the transaction to update.</param>
        /// <param name="newCategoryId">The ID of the new category.</param>
        Task UpdateCategoryAsync(Guid id, Guid newCategoryId);

        /// <summary>
        /// Updates the currency of an existing transaction.
        /// </summary>
        /// <param name="id">The ID of the transaction to update.</param>
        /// <param name="newCurrency">The new currency object.</param>
        Task UpdateCurrencyAsync(Guid id, string newCurrencyCode);

        /// <summary>
        /// Updates the transaction type (income or expense).
        /// </summary>
        /// <param name="id">The ID of the transaction to update.</param>
        /// <param name="newType">The new transaction type.</param>
        Task UpdateTypeAsync(Guid id, TransactionType newType);

        /// <summary>
        /// Updates multiple fields of an existing transaction in a single operation.
        /// </summary>
        /// <param name="id"> The ID of the transaction to update.</param>
        /// <param name="newAmount"> The new amount value, or null to leave unchanged.</param>
        /// <param name="newCategoryId"> The ID of the new category, or null to leave unchanged.</param>
        /// <param name="newCurrencyCode"> The new currency object, or null to leave unchanged.</param>
        /// <param name="newDate"> The new UTC date, or null to leave unchanged.</param>
        /// <param name="newDescription"> The new description text, or null to leave unchanged.</param>
        /// <param name="newType"> The new transaction type, or null to leave unchanged.</param>
        Task UpdateAsync(
            Guid id,
            Guid? newCategoryId = null,
            decimal? newAmount = null,
            string? newCurrencyCode = null,
            string? newDescription = null,
            DateTime? newDate = null,
            TransactionType? newType = null);
    }
}
