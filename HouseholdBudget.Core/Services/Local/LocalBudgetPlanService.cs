using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using HouseholdBudget.Core.Models;
using HouseholdBudget.Core.UserData;
using HouseholdBudget.Core.Data;
using HouseholdBudget.Core.Events.Transactions;
using HouseholdBudget.Core.Services.Interfaces;
using System.Numerics;

namespace HouseholdBudget.Core.Services.Local
{
    /// <summary>
    /// Provides an in-memory implementation of <see cref="IBudgetPlanService"/> for managing budget plans
    /// scoped to the currently authenticated user. Supports lifecycle operations such as creation,
    /// updates, and deletion, and validates all modifications. Reacts to transaction events to keep plans in sync.
    /// </summary>
    public class LocalBudgetPlanService : IBudgetPlanService, ITransactionEventHandler
    {
        private readonly List<BudgetPlan> _plans = new();
        private readonly IUserSessionService _userSession;
        private readonly Lazy<IBudgetExecutionService> _budgetExecutionService;
        private readonly IBudgetRepository _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalBudgetPlanService"/> class.
        /// </summary>
        /// <param name="userSession">Provides access to the currently authenticated user context.</param>
        /// <param name="budgetExecutionService">Lazily resolved execution service used for refreshing plan state.</param>
        /// <param name="budgetRepository">Persistence repository for storing plan data.</param>
        public LocalBudgetPlanService(
            IUserSessionService userSession,
            Lazy<IBudgetExecutionService> budgetExecutionService,
            IBudgetRepository budgetRepository)
        {
            _userSession = userSession;
            _budgetExecutionService = budgetExecutionService;
            _repository = budgetRepository;
        }

        /// <inheritdoc/>
        public Task<IEnumerable<BudgetPlan>> GetAllPlansAsync() =>
            Task.FromResult(_plans.AsEnumerable());

        /// <inheritdoc/>
        public Task<BudgetPlan?> GetByIdAsync(Guid id) =>
            Task.FromResult(_plans.FirstOrDefault(p => p.Id == id));

        /// <inheritdoc/>
        public async Task<BudgetPlan> CreatePlanAsync(
            Guid userId,
            string name,
            DateTime startDate,
            DateTime endDate,
            decimal totalAmount,
            Currency currency,
            string? description = null,
            IEnumerable<CategoryBudgetPlan>? categoryPlans = null)
        {
            EnsureAuthenticated();

            var plan = BudgetPlan.Create(
                userId,
                name,
                startDate,
                endDate,
                totalAmount,
                currency,
                description,
                categoryPlans);

            _plans.Add(plan);

            await _repository.AddBudgetPlanAsync(plan);
            await _repository.SaveChangesAsync();
            await _budgetExecutionService.Value.RefreshExecutionForPlanAsync(plan.Id);

            return plan;
        }

        /// <inheritdoc/>
        public async Task DeletePlanAsync(Guid planId)
        {
            var plan = await RequirePlanAsync(planId);

            await _repository.RemoveBudgetPlanAsync(plan);
            await _repository.SaveChangesAsync();

            _plans.RemoveAll(p => p.Id == planId);
        }

        /// <inheritdoc/>
        public async Task UpdateNameAsync(Guid planId, string newName)
        {
            var plan = await RequirePlanAsync(planId);
            plan.UpdateName(newName);

            await _repository.UpdateBudgetPlanAsync(plan);
            await _repository.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task UpdateDescriptionAsync(Guid planId, string newDescription)
        {
            var plan = await RequirePlanAsync(planId);
            plan.UpdateDescription(newDescription);

            await _repository.UpdateBudgetPlanAsync(plan);
            await _repository.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task UpdateTotalAmountAsync(Guid planId, decimal newTotalAmount)
        {
            var plan = await RequirePlanAsync(planId);
            plan.UpdateTotalAmount(newTotalAmount);

            await _repository.UpdateBudgetPlanAsync(plan);
            await _repository.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task UpdateDatesAsync(Guid planId, DateTime newStartDate, DateTime newEndDate)
        {
            var plan = await RequirePlanAsync(planId);
            plan.UpdateDates(newStartDate, newEndDate);
            await _budgetExecutionService.Value.RefreshExecutionForPlanAsync(plan.Id);

            await _repository.UpdateBudgetPlanAsync(plan);
            await _repository.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task UpdateCurrencyAsync(Guid planId, Currency newCurrency)
        {
            var plan = await RequirePlanAsync(planId);
            plan.UpdateCurrency(newCurrency);
            await _budgetExecutionService.Value.RefreshExecutionForPlanAsync(plan.Id);

            await _repository.UpdateBudgetPlanAsync(plan);
            await _repository.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task UpdateCategoryPlansAsync(Guid planId, IEnumerable<CategoryBudgetPlan> newCategoryPlans)
        {
            var plan = await RequirePlanAsync(planId);
            plan.UpdateCategoryPlans(newCategoryPlans);
            await _budgetExecutionService.Value.RefreshExecutionForPlanAsync(plan.Id);

            await _repository.UpdateBudgetPlanAsync(plan);
            await _repository.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task AddCategoryPlanAsync(Guid planId, CategoryBudgetPlan categoryPlan)
        {
            var plan = await RequirePlanAsync(planId);
            plan.AddCategoryPlan(categoryPlan);
            await _budgetExecutionService.Value.RefreshExecutionForPlanAsync(plan.Id);

            await _repository.UpdateBudgetPlanAsync(plan);
            await _repository.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task RemoveCategoryPlanAsync(Guid planId, Guid categoryId)
        {
            var plan = await RequirePlanAsync(planId);
            plan.RemoveCategoryPlan(categoryId);
            await _budgetExecutionService.Value.RefreshExecutionForPlanAsync(plan.Id);

            await _repository.UpdateBudgetPlanAsync(plan);
            await _repository.SaveChangesAsync();
        }

        /// <summary>
        /// Handles transaction events to update budget plans accordingly.
        /// Currently a placeholder for future implementation.
        /// </summary>
        /// <param name="domainEvent">The transaction domain event to handle.</param>
        public async Task HandleAsync(ITransactionEvent domainEvent)
        {
            switch (domainEvent)
            {
                case TransactionCreated e:
                    await RefreshRelevantPlansAsync(e.Transaction);
                    break;
                case TransactionUpdated e:
                    await RefreshRelevantPlansAsync(e.Transaction);
                    break;
                case TransactionDeleted e:
                    await RefreshRelevantPlansAsync(e.Transaction);
                    break;
                default:
                    break;
            }
        }

        private Task<BudgetPlan> RequirePlanAsync(Guid planId)
        {
            var plan = _plans.FirstOrDefault(p => p.Id == planId);
            if (plan == null)
                throw new InvalidOperationException("Budget plan not found.");

            return Task.FromResult(plan);
        }

        private void EnsureAuthenticated()
        {
            if (_userSession.GetUser() is null)
                throw new InvalidOperationException("User is not authenticated.");
        }

        private async Task RefreshRelevantPlansAsync(Transaction transaction)
        {
            var relevantPlans = _plans.Where(plan =>
                plan.IncludesDate(transaction.Date) &&
                plan.CategoryPlans.Any(cp => cp.CategoryId == transaction.CategoryId)
            );

            foreach (var plan in relevantPlans)
            {
                await _budgetExecutionService.Value.RefreshExecutionForPlanAsync(plan.Id);
            }
        }
    }
}
