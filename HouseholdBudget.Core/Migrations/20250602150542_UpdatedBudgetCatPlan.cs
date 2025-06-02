using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HouseholdBudget.Core.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedBudgetCatPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IncomeExecutedAmount",
                table: "CategoryBudgetPlans",
                newName: "IncomePlanned");

            migrationBuilder.RenameColumn(
                name: "IncomeAmount",
                table: "CategoryBudgetPlans",
                newName: "IncomeExecuted");

            migrationBuilder.RenameColumn(
                name: "ExpenseExecutedAmount",
                table: "CategoryBudgetPlans",
                newName: "ExpensePlanned");

            migrationBuilder.RenameColumn(
                name: "ExpenseAmount",
                table: "CategoryBudgetPlans",
                newName: "ExpenseExecuted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IncomePlanned",
                table: "CategoryBudgetPlans",
                newName: "IncomeExecutedAmount");

            migrationBuilder.RenameColumn(
                name: "IncomeExecuted",
                table: "CategoryBudgetPlans",
                newName: "IncomeAmount");

            migrationBuilder.RenameColumn(
                name: "ExpensePlanned",
                table: "CategoryBudgetPlans",
                newName: "ExpenseExecutedAmount");

            migrationBuilder.RenameColumn(
                name: "ExpenseExecuted",
                table: "CategoryBudgetPlans",
                newName: "ExpenseAmount");
        }
    }
}
