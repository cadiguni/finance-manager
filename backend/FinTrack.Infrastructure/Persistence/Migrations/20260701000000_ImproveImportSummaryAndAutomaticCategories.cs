using FinTrack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinTrack.Infrastructure.Persistence.Migrations;

[DbContext(typeof(FinTrackDbContext))]
[Migration("20260701000000_ImproveImportSummaryAndAutomaticCategories")]
public partial class ImproveImportSummaryAndAutomaticCategories : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<decimal>(
            name: "ExpenseAmount",
            table: "ImportBatches",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: false,
            defaultValue: 0m);

        migrationBuilder.AddColumn<decimal>(
            name: "IncomeAmount",
            table: "ImportBatches",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: false,
            defaultValue: 0m);

        var userId = new Guid("11111111-1111-1111-1111-111111111111");
        var createdAt = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);
        var categoryIds = new[]
        {
            new Guid("33333333-3333-3333-3333-333333333335"),
            new Guid("33333333-3333-3333-3333-333333333336"),
            new Guid("33333333-3333-3333-3333-333333333337"),
            new Guid("33333333-3333-3333-3333-333333333338"),
            new Guid("33333333-3333-3333-3333-333333333339"),
            new Guid("33333333-3333-3333-3333-333333333340")
        };

        migrationBuilder.InsertData(
            table: "Categories",
            columns: new[] { "Id", "CreatedAt", "Name", "ParentCategoryId", "Type", "UserId" },
            values: new object[,]
            {
                { categoryIds[0], createdAt, "Assinaturas e entretenimento", null, "Expense", userId },
                { categoryIds[1], createdAt, "Saude", null, "Expense", userId },
                { categoryIds[2], createdAt, "Compras", null, "Expense", userId },
                { categoryIds[3], createdAt, "Educacao", null, "Expense", userId },
                { categoryIds[4], createdAt, "Seguros", null, "Expense", userId },
                { categoryIds[5], createdAt, "Outros", null, "Expense", userId }
            });

        var foodId = new Guid("33333333-3333-3333-3333-333333333332");
        var rules = new (string Keyword, Guid CategoryId)[]
        {
            ("ifood", foodId), ("restaurante", foodId), ("pizzaria", foodId),
            ("macromix", foodId), ("fardo", foodId), ("aliment", foodId),
            ("crunchyroll", categoryIds[0]), ("youtube", categoryIds[0]),
            ("spotify", categoryIds[0]), ("amazonprime", categoryIds[0]),
            ("steam", categoryIds[0]), ("nuuvem", categoryIds[0]),
            ("wellhub", categoryIds[1]), ("seguro", categoryIds[4]),
            ("uninter", categoryIds[3]), ("mercadolivre", categoryIds[2]),
            ("mercado*", categoryIds[2]), ("amazon", categoryIds[2]),
            ("modas", categoryIds[2]), ("prata fina", categoryIds[2]),
            ("floricultura", categoryIds[2]), ("shop ", categoryIds[2])
        };

        for (var index = 0; index < rules.Length; index++)
        {
            migrationBuilder.InsertData(
                table: "CategoryKeywordRules",
                columns: new[] { "Id", "CategoryId", "CreatedAt", "IsActive", "Keyword", "Priority", "TransactionType", "UserId" },
                values: new object[]
                {
                    Guid.Parse($"55555555-5555-5555-5555-{index + 1:000000000000}"),
                    rules[index].CategoryId,
                    createdAt,
                    true,
                    rules[index].Keyword,
                    100,
                    "Expense",
                    userId
                });
        }
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        for (var index = 0; index < 22; index++)
        {
            migrationBuilder.DeleteData(
                table: "CategoryKeywordRules",
                keyColumn: "Id",
                keyValue: Guid.Parse($"55555555-5555-5555-5555-{index + 1:000000000000}"));
        }

        for (var index = 5; index <= 10; index++)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: index == 10
                    ? new Guid("33333333-3333-3333-3333-333333333340")
                    : Guid.Parse($"33333333-3333-3333-3333-33333333333{index}"));
        }

        migrationBuilder.DropColumn(name: "ExpenseAmount", table: "ImportBatches");
        migrationBuilder.DropColumn(name: "IncomeAmount", table: "ImportBatches");
    }
}
