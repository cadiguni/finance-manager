using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinTrack.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddImportDeduplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_UserId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_ImportBatches_UserId",
                table: "ImportBatches");

            migrationBuilder.AddColumn<string>(
                name: "ImportHash",
                table: "Transactions",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContentHash",
                table: "ImportBatches",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444441"),
                column: "ImportHash",
                value: null);

            migrationBuilder.UpdateData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444442"),
                column: "ImportHash",
                value: null);

            migrationBuilder.UpdateData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444443"),
                column: "ImportHash",
                value: null);

            migrationBuilder.UpdateData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "ImportHash",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_UserId_ImportHash",
                table: "Transactions",
                columns: new[] { "UserId", "ImportHash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImportBatches_UserId_ContentHash",
                table: "ImportBatches",
                columns: new[] { "UserId", "ContentHash" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_UserId_ImportHash",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_ImportBatches_UserId_ContentHash",
                table: "ImportBatches");

            migrationBuilder.DropColumn(
                name: "ImportHash",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "ContentHash",
                table: "ImportBatches");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_UserId",
                table: "Transactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportBatches_UserId",
                table: "ImportBatches",
                column: "UserId");
        }
    }
}
