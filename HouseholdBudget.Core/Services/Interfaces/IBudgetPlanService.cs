using HouseholdBudget.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HouseholdBudget.Core.Services.Interfaces
{
    /// <summary>
    /// Provides high-level functionality for managing budget plans.
    /// A budget plan defines financial goals and category-specific allocations for a specified time range.
    /// This service supports lifecycle operations such as creation, modification, deletion, and retrieval.
    /// </summary>
    public interface IBudgetPlanService
    {
        /// <summary>
        /// Initializes the service by loading all budget plans for the currently authenticated user.
        /// </summary>
        /// <returns></returns>
        Task InitAsync();

        /// <summary>
        /// Retrieves all budget plans defined by the currently authenticated user.
        /// </summary>
        /// <returns>A collection of budget plans belonging to the user.</returns>
        Task<IEnumerable<BudgetPlan>> GetAllPlansAsync();

        /// <summary>
        /// Retrieves a specific budget plan by its unique identifier.
        /// </summary>
        /// <param name="id">The unique ID of the budget plan to retrieve.</param>
        /// <returns>The matching budget plan if found; otherwise, null.</returns>
        Task<BudgetPlan?> GetByIdAsync(Guid id);

        /// <summary>
        /// Creates a new budget plan scoped to the authenticated user.
        /// </summary>
        /// <param name="userId"> The unique identifier of the user for whom the plan is being created.</param>
        /// <param name="startDate">The start date of the planning period.</param>
        /// <param name="endDate">The end date of the planning period.</param>
        /// <param name="totalAmount">The total amount of money allocated across the entire plan.</param>
        /// <param name="currency">The currency in which all amounts are expressed.</param>
        /// <param name="description">An optional description to label or explain the plan.</param>
        /// <param name="categoryPlans">Optional per-category budget allocations associated with the plan.</param>
        /// <returns>The newly created <see cref="BudgetPlan"/> instance.</returns>
        Task<BudgetPlan> CreatePlanAsync(
            Guid userId,
            string name,
            DateTime startDate,
            DateTime endDate,
            string? description = null,
            IEnumerable<CategoryBudgetPlan>? categoryPlans = null);

        /// <summary>
        /// Deletes an existing budget plan by its unique identifier.
        /// </summary>
        /// <param name="planId">The ID of the budget plan to permanently remove.</param>
        Task DeletePlanAsync(Guid planId);

        /// <summary>
        /// Updates the name of an existing budget plan. 
        /// </summary>
        /// <param name="planId"></param>
        /// <param name="newName"></param>
        Task UpdateNameAsync(Guid planId, string newName);

        /// <summary>
        /// Updates the textual description of an existing budget plan.
        /// </summary>
        /// <param name="planId">The ID of the plan to update.</param>
        /// <param name="newDescription">The new description to apply.</param>
        Task UpdateDescriptionAsync(Guid planId, string newDescription);

        /// <summary>
        /// Updates the start and end dates of a budget plan.
        /// </summary>
        /// <param name="planId">The ID of the plan to adjust.</param>
        /// <param name="newStartDate">The new start date to assign.</param>
        /// <param name="newEndDate">The new end date to assign.</param>
        Task UpdateDatesAsync(Guid planId, DateTime newStartDate, DateTime newEndDate);

        /// <summary>
        /// Replaces the full list of category-specific budget allocations for a given plan.
        /// </summary>
        /// <param name="planId">The ID of the plan being updated.</param>
        /// <param name="newCategoryPlans">The full set of new category budget plans.</param>
        Task UpdateCategoryPlansAsync(Guid planId, IEnumerable<CategoryBudgetPlan> newCategoryPlans);

        /// <summary>
        /// Adds a new per-category allocation to the existing plan.
        /// </summary>
        /// <param name="planId">The ID of the plan to which the category plan will be added.</param>
        /// <param name="categoryPlan">The budget allocation for a specific category.</param>
        Task AddCategoryPlanAsync(Guid planId, CategoryBudgetPlan categoryPlan);

        /// <summary>
        /// Removes a specific category allocation from a plan.
        /// </summary>
        /// <param name="planId">The ID of the plan from which to remove the category.</param>
        /// <param name="categoryId">The ID of the category allocation to remove.</param>
        Task RemoveCategoryPlanAsync(Guid planId, Guid categoryId);
    }
}
