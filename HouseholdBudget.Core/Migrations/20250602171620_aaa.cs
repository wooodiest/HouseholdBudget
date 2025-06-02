using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HouseholdBudget.Core.Migrations
{
    /// <inheritdoc />
    public partial class aaa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CategoryBudgetPlans_BudgetPlans_BudgetPlanId",
                table: "CategoryBudgetPlans");

            migrationBuilder.AlterColumn<Guid>(
                name: "BudgetPlanId",
                table: "CategoryBudgetPlans",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CategoryBudgetPlans_BudgetPlans_BudgetPlanId",
                table: "CategoryBudgetPlans",
                column: "BudgetPlanId",
                principalTable: "BudgetPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CategoryBudgetPlans_BudgetPlans_BudgetPlanId",
                table: "CategoryBudgetPlans");

            migrationBuilder.AlterColumn<Guid>(
                name: "BudgetPlanId",
                table: "CategoryBudgetPlans",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AddForeignKey(
                name: "FK_CategoryBudgetPlans_BudgetPlans_BudgetPlanId",
                table: "CategoryBudgetPlans",
                column: "BudgetPlanId",
                principalTable: "BudgetPlans",
                principalColumn: "Id");
        }
    }
}
