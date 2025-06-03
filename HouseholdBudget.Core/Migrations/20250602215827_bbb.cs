using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HouseholdBudget.Core.Migrations
{
    /// <inheritdoc />
    public partial class bbb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CategoryBudgetPlans_Currencies_CurrencyId",
                table: "CategoryBudgetPlans");

            migrationBuilder.DropIndex(
                name: "IX_CategoryBudgetPlans_CurrencyId",
                table: "CategoryBudgetPlans");

            migrationBuilder.RenameColumn(
                name: "CurrencyId",
                table: "CategoryBudgetPlans",
                newName: "CurrencyCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CurrencyCode",
                table: "CategoryBudgetPlans",
                newName: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryBudgetPlans_CurrencyId",
                table: "CategoryBudgetPlans",
                column: "CurrencyId");

            migrationBuilder.AddForeignKey(
                name: "FK_CategoryBudgetPlans_Currencies_CurrencyId",
                table: "CategoryBudgetPlans",
                column: "CurrencyId",
                principalTable: "Currencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
