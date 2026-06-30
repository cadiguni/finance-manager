using FinTrack.Application.Categorization;
using FinTrack.Application.Accounts;
using FinTrack.Application.Categories;
using FinTrack.Application.Imports;
using FinTrack.Application.Transactions;
using FinTrack.Domain.Entities;
using FinTrack.Domain.Enums;
using System.IO.Compression;
using System.Security;
using System.Text;

namespace FinTrack.Application.Tests.Imports;

public class ImportServiceExcelTests
{
    [Fact]
    public async Task PreviewExcelAsync_WhenWorkbookIsValid_ReturnsValidatedRows()
    {
        var fixture = TestFixture.Create();
        var content = CreateWorkbookBase64(
            new[]
            {
                new[] { "description", "amount", "type", "date", "accountId", "categoryId", "dueDate", "isPaid", "paymentDate" },
                new[]
                {
                    "Mercado",
                    "120.50",
                    "Expense",
                    "2026-06-01",
                    fixture.Account.Id.ToString(),
                    fixture.Category.Id.ToString(),
                    "2026-06-05",
                    "false",
                    ""
                }
            });

        var preview = await fixture.Service.PreviewExcelAsync(
            fixture.UserId,
            new ExcelPreviewRequest("transactions.xlsx", content, null),
            CancellationToken.None);

        Assert.Equal(1, preview.TotalRows);
        Assert.Equal(1, preview.ValidRows);
        Assert.Empty(preview.Rows[0].Errors);
        Assert.Equal("Mercado", preview.Rows[0].Description);
    }

    [Fact]
    public async Task CommitExcelAsync_WhenWorkbookHasValidRows_AddsTransactionsAndBatch()
    {
        var fixture = TestFixture.Create();
        var content = CreateWorkbookBase64(
            new[]
            {
                new[] { "description", "amount", "type", "date", "accountId", "categoryId" },
                new[]
                {
                    "Salario",
                    "5000",
                    "Income",
                    "2026-06-01",
                    fixture.Account.Id.ToString(),
                    fixture.IncomeCategory.Id.ToString()
                }
            });

        var result = await fixture.Service.CommitExcelAsync(
            fixture.UserId,
            new CommitExcelImportRequest("income.xlsx", content, null),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(fixture.TransactionRepository.Transactions);
        Assert.Single(fixture.ImportBatchRepository.Batches);
        Assert.Equal(FileImportType.Excel, fixture.ImportBatchRepository.Batches[0].FileType);
        Assert.Equal("Salario", fixture.TransactionRepository.Transactions[0].Description);
    }

    [Fact]
    public async Task PreviewExcelAsync_WhenCategoryIsEmptyAndKeywordMatches_UsesMatchedCategory()
    {
        var fixture = TestFixture.Create();
        fixture.KeywordRuleRepository.Rules.Add(new CategoryKeywordRule
        {
            UserId = fixture.UserId,
            CategoryId = fixture.Category.Id,
            Category = fixture.Category,
            Keyword = "mercado",
            TransactionType = TransactionType.Expense,
            Priority = 10,
            IsActive = true
        });
        var content = CreateWorkbookBase64(
            new[]
            {
                new[] { "description", "amount", "type", "date", "accountId", "categoryId" },
                new[]
                {
                    "Mercado Central",
                    "120.50",
                    "Expense",
                    "2026-06-01",
                    fixture.Account.Id.ToString(),
                    ""
                }
            });

        var preview = await fixture.Service.PreviewExcelAsync(
            fixture.UserId,
            new ExcelPreviewRequest("transactions.xlsx", content, null),
            CancellationToken.None);

        Assert.Equal(1, preview.ValidRows);
        Assert.Equal(fixture.Category.Id, preview.Rows[0].CategoryId);
        Assert.Empty(preview.Rows[0].Errors);
    }

    [Fact]
    public async Task PreviewCsvAsync_WhenBankExportHasDateTitleAmount_UsesDefaultsAndSkipsCardPayment()
    {
        var fixture = TestFixture.Create();
        var content =
            """
            date,title,amount
            2026-06-10,Pix no Crédito - UNINTER,"121,39"
            2026-06-09,Dm*Spotify,"23,90"
            2026-06-01,Pagamento recebido,"- 1.237,11"
            """;

        var preview = await fixture.Service.PreviewCsvAsync(
            fixture.UserId,
            new CsvPreviewRequest(
                "nubank.csv",
                content,
                fixture.Account.Id,
                fixture.Category.Id),
            CancellationToken.None);

        Assert.Equal(2, preview.TotalRows);
        Assert.Equal(2, preview.ValidRows);
        Assert.All(preview.Rows, row => Assert.Empty(row.Errors));
        Assert.Equal("Pix no Crédito - UNINTER", preview.Rows[0].Description);
        Assert.Equal(121.39m, preview.Rows[0].Amount);
        Assert.Equal(TransactionType.Expense, preview.Rows[0].Type);
        Assert.Equal(fixture.Account.Id, preview.Rows[0].AccountId);
        Assert.Equal(fixture.Category.Id, preview.Rows[0].CategoryId);
    }

    [Fact]
    public async Task CommitCsvAsync_WhenCsvIsValid_ImportsTransactionOnce()
    {
        var fixture = TestFixture.Create();
        var request = CreateCsvRequest(
            "transactions.csv",
            fixture.Account.Id,
            fixture.Category.Id);

        var result = await fixture.Service.CommitCsvAsync(
            fixture.UserId,
            request,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(fixture.TransactionRepository.Transactions);
        Assert.NotNull(fixture.TransactionRepository.Transactions[0].ImportHash);
        Assert.Single(fixture.ImportBatchRepository.Batches);
        Assert.False(string.IsNullOrEmpty(fixture.ImportBatchRepository.Batches[0].ContentHash));
    }

    [Fact]
    public async Task CommitCsvAsync_WhenSameFileIsImportedTwice_DoesNotDuplicateTransactions()
    {
        var fixture = TestFixture.Create();
        var request = CreateCsvRequest(
            "transactions.csv",
            fixture.Account.Id,
            fixture.Category.Id);

        var firstResult = await fixture.Service.CommitCsvAsync(
            fixture.UserId,
            request,
            CancellationToken.None);
        var secondResult = await fixture.Service.CommitCsvAsync(
            fixture.UserId,
            request,
            CancellationToken.None);

        Assert.True(firstResult.IsSuccess);
        Assert.False(secondResult.IsSuccess);
        Assert.Equal("Este arquivo já foi importado.", secondResult.Error);
        Assert.Single(fixture.TransactionRepository.Transactions);
        Assert.Single(fixture.ImportBatchRepository.Batches);
    }

    [Fact]
    public async Task CommitCsvAsync_WhenDifferentFileContainsImportedRow_SkipsDuplicateRow()
    {
        var fixture = TestFixture.Create();
        var firstRequest = CreateCsvRequest(
            "first.csv",
            fixture.Account.Id,
            fixture.Category.Id);
        var secondRequest = CreateCsvRequest(
            "second.csv",
            fixture.Account.Id,
            fixture.Category.Id,
            trailingNewLine: true);

        await fixture.Service.CommitCsvAsync(fixture.UserId, firstRequest, CancellationToken.None);
        var preview = await fixture.Service.PreviewCsvAsync(
            fixture.UserId,
            new CsvPreviewRequest(
                secondRequest.FileName,
                secondRequest.Content,
                secondRequest.DefaultAccountId,
                secondRequest.DefaultCategoryId),
            CancellationToken.None);
        var secondResult = await fixture.Service.CommitCsvAsync(
            fixture.UserId,
            secondRequest,
            CancellationToken.None);

        Assert.False(secondResult.IsSuccess);
        Assert.Single(fixture.TransactionRepository.Transactions);
        Assert.Equal(0, preview.ValidRows);
        Assert.Contains("Este lançamento já foi importado.", preview.Rows[0].Errors);
    }

    [Fact]
    public async Task PreviewCardStatementAsync_WhenKeywordMatches_UsesMatchedCategory()
    {
        var fixture = TestFixture.Create();
        fixture.KeywordRuleRepository.Rules.Add(new CategoryKeywordRule
        {
            UserId = fixture.UserId,
            CategoryId = fixture.Category.Id,
            Category = fixture.Category,
            Keyword = "mercado",
            TransactionType = TransactionType.Expense,
            Priority = 10,
            IsActive = true
        });

        var preview = await fixture.Service.PreviewCardStatementAsync(
            fixture.UserId,
            new CardStatementPreviewRequest(
                "fatura.txt",
                """
                Data Descricao Valor
                01/06 Mercado Central R$ 120,50
                02/06 App Corrida 35.90
                """,
                null,
                fixture.Account.Id,
                new DateOnly(2026, 6, 10),
                false,
                null),
            CancellationToken.None);

        Assert.Equal(2, preview.TotalRows);
        Assert.Equal(1, preview.ValidRows);
        Assert.Equal(fixture.Category.Id, preview.Rows[0].CategoryId);
        Assert.Equal(new DateOnly(2026, 6, 1), preview.Rows[0].Date);
        Assert.Contains("Category not found.", preview.Rows[1].Errors);
    }

    [Fact]
    public async Task CommitCardStatementAsync_WhenRowsAreValid_AddsTransactionsAndPdfBatch()
    {
        var fixture = TestFixture.Create();
        fixture.KeywordRuleRepository.Rules.Add(new CategoryKeywordRule
        {
            UserId = fixture.UserId,
            CategoryId = fixture.Category.Id,
            Category = fixture.Category,
            Keyword = "mercado",
            TransactionType = TransactionType.Expense,
            Priority = 10,
            IsActive = true
        });

        var result = await fixture.Service.CommitCardStatementAsync(
            fixture.UserId,
            new CommitCardStatementImportRequest(
                "fatura-cartao.txt",
                "01/06 Mercado Central R$ 120,50",
                null,
                fixture.Account.Id,
                new DateOnly(2026, 6, 10),
                true,
                new DateOnly(2026, 6, 10)),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(fixture.TransactionRepository.Transactions);
        Assert.Single(fixture.ImportBatchRepository.Batches);
        Assert.Equal(FileImportType.Pdf, fixture.ImportBatchRepository.Batches[0].FileType);
        Assert.Equal("Mercado Central", fixture.TransactionRepository.Transactions[0].Description);
        Assert.Equal(120.50m, fixture.TransactionRepository.Transactions[0].Amount);
        Assert.True(fixture.TransactionRepository.Transactions[0].IsPaid);
    }

    private sealed record TestFixture(
        Guid UserId,
        ImportService Service,
        Account Account,
        Category Category,
        Category IncomeCategory,
        FakeTransactionRepository TransactionRepository,
        FakeImportBatchRepository ImportBatchRepository,
        FakeCategoryKeywordRuleRepository KeywordRuleRepository)
    {
        public static TestFixture Create()
        {
            var userId = Guid.NewGuid();
            var account = new Account
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = "Conta",
                Type = AccountType.BankAccount
            };
            var category = new Category
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = "Alimentacao",
                Type = CategoryType.Expense
            };
            var incomeCategory = new Category
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = "Salario",
                Type = CategoryType.Income
            };

            var importBatchRepository = new FakeImportBatchRepository();
            var transactionRepository = new FakeTransactionRepository();
            var keywordRuleRepository = new FakeCategoryKeywordRuleRepository();
            var service = new ImportService(
                importBatchRepository,
                transactionRepository,
                new FakeAccountRepository(new[] { account }),
                new FakeCategoryRepository(new[] { category, incomeCategory }),
                keywordRuleRepository);

            return new TestFixture(
                userId,
                service,
                account,
                category,
                incomeCategory,
                transactionRepository,
                importBatchRepository,
                keywordRuleRepository);
        }
    }

    private static CommitCsvImportRequest CreateCsvRequest(
        string fileName,
        Guid accountId,
        Guid categoryId,
        bool trailingNewLine = false)
    {
        var content = "description,amount,type,date\nMercado,120.50,Expense,2026-06-10";
        if (trailingNewLine)
        {
            content += "\n";
        }

        return new CommitCsvImportRequest(fileName, content, accountId, categoryId);
    }

    private sealed class FakeCategoryKeywordRuleRepository : ICategoryKeywordRuleRepository
    {
        public List<CategoryKeywordRule> Rules { get; } = new();

        public Task<IReadOnlyList<CategoryKeywordRule>> GetAllAsync(Guid userId, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<CategoryKeywordRule>>(
                Rules.Where(rule => rule.UserId == userId).ToList());
        }

        public Task<CategoryKeywordRule?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(Rules.FirstOrDefault(rule => rule.UserId == userId && rule.Id == id));
        }

        public Task<CategoryKeywordRule?> FindMatchAsync(
            Guid userId,
            string description,
            TransactionType transactionType,
            CancellationToken cancellationToken)
        {
            var match = Rules
                .Where(rule =>
                    rule.UserId == userId &&
                    rule.IsActive &&
                    (rule.TransactionType is null || rule.TransactionType == transactionType))
                .OrderByDescending(rule => rule.Priority)
                .ThenByDescending(rule => rule.Keyword.Length)
                .FirstOrDefault(rule =>
                    description.Contains(rule.Keyword, StringComparison.OrdinalIgnoreCase));

            return Task.FromResult(match);
        }

        public Task<bool> ExistsAsync(
            Guid userId,
            string keyword,
            TransactionType? transactionType,
            Guid? exceptId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(Rules.Any(rule =>
                rule.UserId == userId &&
                rule.Id != exceptId &&
                string.Equals(rule.Keyword, keyword, StringComparison.OrdinalIgnoreCase) &&
                rule.TransactionType == transactionType));
        }

        public Task AddAsync(CategoryKeywordRule rule, CancellationToken cancellationToken)
        {
            Rules.Add(rule);
            return Task.CompletedTask;
        }

        public void Remove(CategoryKeywordRule rule)
        {
            Rules.Remove(rule);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FakeImportBatchRepository : IImportBatchRepository
    {
        public List<ImportBatch> Batches { get; } = new();

        public Task<IReadOnlyList<ImportBatch>> GetAllAsync(Guid userId, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<ImportBatch>>(
                Batches.Where(batch => batch.UserId == userId).ToList());
        }

        public Task AddAsync(ImportBatch batch, CancellationToken cancellationToken)
        {
            Batches.Add(batch);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsByContentHashAsync(
            Guid userId,
            string contentHash,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(Batches.Any(batch =>
                batch.UserId == userId && batch.ContentHash == contentHash));
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FakeTransactionRepository : ITransactionRepository
    {
        public List<Transaction> Transactions { get; } = new();

        public Task AddAsync(Transaction transaction, CancellationToken cancellationToken)
        {
            Transactions.Add(transaction);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<Transaction>> GetAllAsync(
            Guid userId,
            TransactionFilters filters,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<Transaction>>(
                Transactions.Where(transaction => transaction.UserId == userId).ToList());
        }

        public Task<Transaction?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(
                Transactions.FirstOrDefault(transaction => transaction.UserId == userId && transaction.Id == id));
        }

        public Task<bool> ExistsByImportHashAsync(Guid userId, string importHash, CancellationToken cancellationToken)
        {
            return Task.FromResult(Transactions.Any(transaction =>
                transaction.UserId == userId && transaction.ImportHash == importHash));
        }

        public void Remove(Transaction transaction)
        {
            Transactions.Remove(transaction);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FakeAccountRepository : IAccountRepository
    {
        private readonly List<Account> _accounts;

        public FakeAccountRepository(IEnumerable<Account> accounts)
        {
            _accounts = accounts.ToList();
        }

        public Task AddAsync(Account account, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<IReadOnlyList<Account>> GetAllAsync(Guid userId, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<Account>>(_accounts);
        }

        public Task<Account?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_accounts.FirstOrDefault(account => account.UserId == userId && account.Id == id));
        }

        public Task<bool> HasTransactionsAsync(Guid userId, Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(false);
        }

        public Task<bool> HasRecurringRulesAsync(Guid userId, Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(false);
        }

        public void Remove(Account account)
        {
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FakeCategoryRepository : ICategoryRepository
    {
        private readonly List<Category> _categories;

        public FakeCategoryRepository(IEnumerable<Category> categories)
        {
            _categories = categories.ToList();
        }

        public Task AddAsync(Category category, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<bool> ExistsAsync(Guid userId, Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_categories.Any(category => category.UserId == userId && category.Id == id));
        }

        public Task<IReadOnlyList<Category>> GetAllAsync(Guid userId, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<Category>>(_categories);
        }

        public Task<Category?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_categories.FirstOrDefault(category => category.UserId == userId && category.Id == id));
        }

        public Task<bool> HasChildrenAsync(Guid userId, Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(false);
        }

        public Task<bool> HasTransactionsAsync(Guid userId, Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(false);
        }

        public Task<bool> HasRecurringRulesAsync(Guid userId, Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(false);
        }

        public Task<bool> HasKeywordRulesAsync(Guid userId, Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(false);
        }

        public void Remove(Category category)
        {
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private static string CreateWorkbookBase64(IReadOnlyList<IReadOnlyList<string>> rows)
    {
        using var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
        {
            WriteEntry(
                archive,
                "[Content_Types].xml",
                """
                <?xml version="1.0" encoding="UTF-8"?>
                <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
                  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
                  <Default Extension="xml" ContentType="application/xml"/>
                  <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
                  <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
                </Types>
                """);
            WriteEntry(
                archive,
                "_rels/.rels",
                """
                <?xml version="1.0" encoding="UTF-8"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
                </Relationships>
                """);
            WriteEntry(
                archive,
                "xl/workbook.xml",
                """
                <?xml version="1.0" encoding="UTF-8"?>
                <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                  <sheets>
                    <sheet name="Transactions" sheetId="1" r:id="rId1"/>
                  </sheets>
                </workbook>
                """);
            WriteEntry(
                archive,
                "xl/_rels/workbook.xml.rels",
                """
                <?xml version="1.0" encoding="UTF-8"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
                </Relationships>
                """);
            WriteEntry(archive, "xl/worksheets/sheet1.xml", BuildSheetXml(rows));
        }

        return Convert.ToBase64String(stream.ToArray());
    }

    private static string BuildSheetXml(IReadOnlyList<IReadOnlyList<string>> rows)
    {
        var builder = new StringBuilder();
        builder.AppendLine("""<?xml version="1.0" encoding="UTF-8"?>""");
        builder.AppendLine("""<worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">""");
        builder.AppendLine("<sheetData>");

        for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        {
            builder.AppendLine($"""<row r="{rowIndex + 1}">""");
            for (var columnIndex = 0; columnIndex < rows[rowIndex].Count; columnIndex++)
            {
                var reference = $"{GetColumnName(columnIndex)}{rowIndex + 1}";
                var value = SecurityElement.Escape(rows[rowIndex][columnIndex]) ?? string.Empty;
                builder.AppendLine($"""<c r="{reference}" t="inlineStr"><is><t>{value}</t></is></c>""");
            }

            builder.AppendLine("</row>");
        }

        builder.AppendLine("</sheetData>");
        builder.AppendLine("</worksheet>");
        return builder.ToString();
    }

    private static string GetColumnName(int columnIndex)
    {
        var value = columnIndex + 1;
        var name = string.Empty;
        while (value > 0)
        {
            value--;
            name = (char)('A' + value % 26) + name;
            value /= 26;
        }

        return name;
    }

    private static void WriteEntry(ZipArchive archive, string path, string content)
    {
        var entry = archive.CreateEntry(path);
        using var stream = entry.Open();
        using var writer = new StreamWriter(stream, Encoding.UTF8);
        writer.Write(content);
    }
}
