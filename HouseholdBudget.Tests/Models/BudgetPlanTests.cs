using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using HouseholdBudget.Core.Models;
using Xunit;

namespace HouseholdBudget.Tests.Models;

public class BudgetPlanTests
{
    private readonly Currency _usd = Currency.Create("USD", "$", "US Dollar");

    [Fact]
    public void CategoryBudgetPlan_ShouldThrow_WhenCategoryIdIsEmpty()
    {
        Action act = () => new CategoryBudgetPlan(Guid.Empty, 100, _usd);

        act.Should().Throw<ValidationException>()
           .WithMessage("*CategoryId*");
    }

    [Fact]
    public void CategoryBudgetPlan_ShouldThrow_WhenAmountIsNegative()
    {
        Action act = () => new CategoryBudgetPlan(Guid.NewGuid(), -1, _usd);

        act.Should().Throw<ValidationException>()
           .WithMessage("*non-negative*");
    }

    [Fact]
    public void CategoryBudgetPlan_ShouldThrow_WhenCurrencyIsNull()
    {
        Action act = () => new CategoryBudgetPlan(Guid.NewGuid(), 10, null!);

        act.Should().Throw<ValidationException>()
           .WithMessage("*Currency*");
    }

    [Fact]
    public void CategoryBudgetPlan_ShouldCreate_WhenValidInput()
    {
        var categoryId = Guid.NewGuid();
        var plan = new CategoryBudgetPlan(categoryId, 50, _usd);

        plan.CategoryId.Should().Be(categoryId);
        plan.Amount.Should().Be(50);
        plan.Currency.Should().Be(_usd);
    }

    [Fact]
    public void BudgetPlan_Create_ShouldThrow_WhenStartDateAfterEndDate()
    {
        var start = DateTime.Today;
        var end = start.AddDays(-1);

        Action act = () => BudgetPlan.Create(start, end, 100, _usd);

        act.Should().Throw<ValidationException>()
           .WithMessage("*StartDate must be earlier*");
    }

    [Fact]
    public void BudgetPlan_Create_ShouldThrow_WhenTotalAmountNegative()
    {
        var start = DateTime.Today;
        var end = start.AddDays(1);

        Action act = () => BudgetPlan.Create(start, end, -5, _usd);

        act.Should().Throw<ValidationException>()
           .WithMessage("*non-negative*");
    }

    [Fact]
    public void BudgetPlan_Create_ShouldThrow_WhenCurrencyIsNull()
    {
        var start = DateTime.Today;
        var end = start.AddDays(1);

        Action act = () => BudgetPlan.Create(start, end, 10, null!);

        act.Should().Throw<ValidationException>()
           .WithMessage("*Currency is required*");
    }

    [Fact]
    public void BudgetPlan_Create_ShouldThrow_WhenDescriptionTooLong()
    {
        var longDesc = new string('x', BudgetPlan.MaxDescriptionLength + 1);

        Action act = () => BudgetPlan.Create(DateTime.Today, DateTime.Today, 10, _usd, longDesc);

        act.Should().Throw<ValidationException>()
           .WithMessage($"*Description cannot exceed*");
    }

    [Fact]
    public void BudgetPlan_Create_ShouldSucceed_WhenValidInput()
    {
        var categoryId = Guid.NewGuid();
        var categoryPlans = new List<CategoryBudgetPlan>
        {
            new(categoryId, 100, _usd)
        };

        var plan = BudgetPlan.Create(DateTime.Today, DateTime.Today.AddDays(10), 500, _usd, "My Budget", categoryPlans);

        plan.Description.Should().Be("My Budget");
        plan.TotalAmount.Should().Be(500);
        plan.Currency.Code.Should().Be("USD");
        plan.CategoryPlans.Should().HaveCount(1);
    }

    [Fact]
    public void BudgetPlan_Validate_ShouldReturnAllErrors()
    {
        var errors = BudgetPlan.Validate(
            DateTime.Today.AddDays(5), // start after end
            DateTime.Today,
            -10,                      // negative total
            null,                     // null currency
            new string('x', 300));    // too long description

        errors.Should().HaveCount(4);
        errors.Should().Contain(e => e.Contains("StartDate"));
        errors.Should().Contain(e => e.Contains("TotalAmount"));
        errors.Should().Contain(e => e.Contains("Currency"));
        errors.Should().Contain(e => e.Contains("Description"));
    }

    [Theory]
    [InlineData("2024-01-01", "2024-01-31", "2024-01-15", true)]
    [InlineData("2024-01-01", "2024-01-31", "2024-02-01", false)]
    public void BudgetPlan_IncludesDate_ShouldReturnCorrectValue(string start, string end, string testDate, bool expected)
    {
        var plan = BudgetPlan.Create(
            DateTime.Parse(start),
            DateTime.Parse(end),
            1000,
            _usd);

        plan.IncludesDate(DateTime.Parse(testDate)).Should().Be(expected);
    }

}
