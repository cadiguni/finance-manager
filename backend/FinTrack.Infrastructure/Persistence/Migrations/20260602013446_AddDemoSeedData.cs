using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FinTrack.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDemoSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Accounts",
                columns: new[] { "Id", "CreatedAt", "InitialBalance", "Name", "Type", "UserId" },
                values: new object[,]
                {
                    { new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2500m, "Conta Principal", "BankAccount", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("22222222-2222-2222-2222-222222222223"), new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), 300m, "Carteira", "Cash", new Guid("11111111-1111-1111-1111-111111111111") }
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAt", "Name", "ParentCategoryId", "Type", "UserId" },
                values: new object[,]
                {
                    { new Guid("33333333-3333-3333-3333-333333333331"), new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Salario", null, "Income", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("33333333-3333-3333-3333-333333333332"), new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Alimentacao", null, "Expense", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Moradia", null, "Expense", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("33333333-3333-3333-3333-333333333334"), new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Transporte", null, "Expense", new Guid("11111111-1111-1111-1111-111111111111") }
                });

            migrationBuilder.InsertData(
                table: "Transactions",
                columns: new[] { "Id", "AccountId", "Amount", "CategoryId", "CreatedAt", "Date", "Description", "DueDate", "InstallmentGroupId", "IsPaid", "PaymentDate", "RecurringRuleId", "Type", "UserId" },
                values: new object[,]
                {
                    { new Guid("44444444-4444-4444-4444-444444444441"), new Guid("22222222-2222-2222-2222-222222222222"), 6500m, new Guid("33333333-3333-3333-3333-333333333331"), new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2026, 6, 5), "Salario mensal", null, null, true, new DateOnly(2026, 6, 5), null, "Income", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("44444444-4444-4444-4444-444444444442"), new Guid("22222222-2222-2222-2222-222222222222"), 420.75m, new Guid("33333333-3333-3333-3333-333333333332"), new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2026, 6, 7), "Mercado", new DateOnly(2026, 6, 7), null, true, new DateOnly(2026, 6, 7), null, "Expense", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("44444444-4444-4444-4444-444444444443"), new Guid("22222222-2222-2222-2222-222222222222"), 1800m, new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2026, 6, 10), "Aluguel", new DateOnly(2026, 6, 10), null, false, null, null, "Expense", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("44444444-4444-4444-4444-444444444444"), new Guid("22222222-2222-2222-2222-222222222223"), 85.50m, new Guid("33333333-3333-3333-3333-333333333334"), new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateOnly(2026, 6, 12), "Transporte semanal", new DateOnly(2026, 6, 12), null, false, null, null, "Expense", new Guid("11111111-1111-1111-1111-111111111111") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444441"));

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444442"));

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444443"));

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222223"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333331"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333332"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333334"));
        }
    }
}
