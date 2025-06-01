using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HouseholdBudget.Core.Migrations
{
    /// <inheritdoc />
    public partial class ModifiedBudgetPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BudgetPlans_Currencies_CurrencyId",
                table: "BudgetPlans");

            migrationBuilder.DropIndex(
                name: "IX_BudgetPlans_CurrencyId",
                table: "BudgetPlans");

            migrationBuilder.DropColumn(
                name: "CurrencyId",
                table: "BudgetPlans");

            migrationBuilder.DropColumn(
                name: "ExecutedAmount",
                table: "BudgetPlans");

            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "BudgetPlans");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CurrencyId",
                table: "BudgetPlans",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "ExecutedAmount",
                table: "BudgetPlans",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "BudgetPlans",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_BudgetPlans_CurrencyId",
                table: "BudgetPlans",
                column: "CurrencyId");

            migrationBuilder.AddForeignKey(
                name: "FK_BudgetPlans_Currencies_CurrencyId",
                table: "BudgetPlans",
                column: "CurrencyId",
                principalTable: "Currencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
