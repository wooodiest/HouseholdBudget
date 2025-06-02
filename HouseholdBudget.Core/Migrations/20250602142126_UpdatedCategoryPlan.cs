using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HouseholdBudget.Core.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedCategoryPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ExecutedAmount",
                table: "CategoryBudgetPlans",
                newName: "IncomeExecutedAmount");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "CategoryBudgetPlans",
                newName: "IncomeAmount");

            migrationBuilder.AddColumn<decimal>(
                name: "ExpenseAmount",
                table: "CategoryBudgetPlans",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ExpenseExecutedAmount",
                table: "CategoryBudgetPlans",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpenseAmount",
                table: "CategoryBudgetPlans");

            migrationBuilder.DropColumn(
                name: "ExpenseExecutedAmount",
                table: "CategoryBudgetPlans");

            migrationBuilder.RenameColumn(
                name: "IncomeExecutedAmount",
                table: "CategoryBudgetPlans",
                newName: "ExecutedAmount");

            migrationBuilder.RenameColumn(
                name: "IncomeAmount",
                table: "CategoryBudgetPlans",
                newName: "Amount");
        }
    }
}
