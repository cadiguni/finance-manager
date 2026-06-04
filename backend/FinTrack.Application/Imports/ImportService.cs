using FinTrack.Application.Categorization;
using FinTrack.Application.Accounts;
using FinTrack.Application.Categories;
using FinTrack.Application.Common;
using FinTrack.Application.Transactions;
using FinTrack.Domain.Entities;
using FinTrack.Domain.Enums;
using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;

namespace FinTrack.Application.Imports;

public sealed class ImportService : IImportService
{
    private static readonly string[] RequiredColumns =
    {
        "description",
        "amount",
        "type",
        "date",
        "accountId"
    };

    private readonly IImportBatchRepository _importBatchRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICategoryKeywordRuleRepository _categoryKeywordRuleRepository;

    public ImportService(
        IImportBatchRepository importBatchRepository,
        ITransactionRepository transactionRepository,
        IAccountRepository accountRepository,
        ICategoryRepository categoryRepository,
        ICategoryKeywordRuleRepository categoryKeywordRuleRepository)
    {
        _importBatchRepository = importBatchRepository;
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
        _categoryRepository = categoryRepository;
        _categoryKeywordRuleRepository = categoryKeywordRuleRepository;
    }

    public async Task<CsvImportPreviewDto> PreviewCsvAsync(
        Guid userId,
        CsvPreviewRequest request,
        CancellationToken cancellationToken)
    {
        var rows = await ParseAndValidateAsync(userId, ParseCsv(request.Content), cancellationToken);
        return BuildPreview(rows);
    }

    public async Task<Result<ImportBatchDto>> CommitCsvAsync(
        Guid userId,
        CommitCsvImportRequest request,
        CancellationToken cancellationToken)
    {
        var rows = await ParseAndValidateAsync(userId, ParseCsv(request.Content), cancellationToken);
        var validRows = rows.Where(row => row.Errors.Count == 0).ToList();

        var batch = new ImportBatch
        {
            UserId = userId,
            FileName = string.IsNullOrWhiteSpace(request.FileName) ? "import.csv" : request.FileName.Trim(),
            FileType = FileImportType.Csv,
            ImportedAt = DateTime.UtcNow,
            TotalRows = rows.Count,
            SuccessRows = validRows.Count,
            FailedRows = rows.Count - validRows.Count,
            Status = validRows.Count == 0 ? FileImportStatus.Failed : FileImportStatus.Completed
        };

        await _importBatchRepository.AddAsync(batch, cancellationToken);

        foreach (var row in validRows)
        {
            await _transactionRepository.AddAsync(new Transaction
            {
                UserId = userId,
                AccountId = row.AccountId!.Value,
                CategoryId = row.CategoryId!.Value,
                Description = row.Description,
                Amount = row.Amount!.Value,
                Type = row.Type!.Value,
                Date = row.Date!.Value,
                DueDate = row.DueDate,
                IsPaid = row.IsPaid ?? false,
                PaymentDate = row.PaymentDate
            }, cancellationToken);
        }

        await _transactionRepository.SaveChangesAsync(cancellationToken);

        return validRows.Count == 0
            ? Result<ImportBatchDto>.Failure("CSV import has no valid rows.")
            : Result<ImportBatchDto>.Success(MapToDto(batch));
    }

    public async Task<CsvImportPreviewDto> PreviewExcelAsync(
        Guid userId,
        ExcelPreviewRequest request,
        CancellationToken cancellationToken)
    {
        var rows = await ParseAndValidateAsync(
            userId,
            ParseExcel(request.ContentBase64, request.WorksheetName),
            cancellationToken);

        return BuildPreview(rows);
    }

    public async Task<Result<ImportBatchDto>> CommitExcelAsync(
        Guid userId,
        CommitExcelImportRequest request,
        CancellationToken cancellationToken)
    {
        var rows = await ParseAndValidateAsync(
            userId,
            ParseExcel(request.ContentBase64, request.WorksheetName),
            cancellationToken);
        var validRows = rows.Where(row => row.Errors.Count == 0).ToList();

        var batch = new ImportBatch
        {
            UserId = userId,
            FileName = string.IsNullOrWhiteSpace(request.FileName) ? "import.xlsx" : request.FileName.Trim(),
            FileType = FileImportType.Excel,
            ImportedAt = DateTime.UtcNow,
            TotalRows = rows.Count,
            SuccessRows = validRows.Count,
            FailedRows = rows.Count - validRows.Count,
            Status = validRows.Count == 0 ? FileImportStatus.Failed : FileImportStatus.Completed
        };

        await _importBatchRepository.AddAsync(batch, cancellationToken);

        foreach (var row in validRows)
        {
            await _transactionRepository.AddAsync(new Transaction
            {
                UserId = userId,
                AccountId = row.AccountId!.Value,
                CategoryId = row.CategoryId!.Value,
                Description = row.Description,
                Amount = row.Amount!.Value,
                Type = row.Type!.Value,
                Date = row.Date!.Value,
                DueDate = row.DueDate,
                IsPaid = row.IsPaid ?? false,
                PaymentDate = row.PaymentDate
            }, cancellationToken);
        }

        await _transactionRepository.SaveChangesAsync(cancellationToken);

        return validRows.Count == 0
            ? Result<ImportBatchDto>.Failure("Excel import has no valid rows.")
            : Result<ImportBatchDto>.Success(MapToDto(batch));
    }

    public async Task<IReadOnlyList<ImportBatchDto>> GetHistoryAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var batches = await _importBatchRepository.GetAllAsync(userId, cancellationToken);
        return batches.Select(MapToDto).ToList();
    }

    private async Task<IReadOnlyList<CsvImportRowPreviewDto>> ParseAndValidateAsync(
        Guid userId,
        List<List<string>> records,
        CancellationToken cancellationToken)
    {
        if (records.Count == 0)
        {
            return Array.Empty<CsvImportRowPreviewDto>();
        }

        var header = records[0]
            .Select((name, index) => new { Name = name.Trim(), Index = index })
            .ToDictionary(column => column.Name, column => column.Index, StringComparer.OrdinalIgnoreCase);

        var missingColumns = RequiredColumns
            .Where(column => !header.ContainsKey(column))
            .ToList();

        var rows = new List<CsvImportRowPreviewDto>();
        for (var index = 1; index < records.Count; index++)
        {
            var values = records[index];
            var errors = new List<string>();
            errors.AddRange(missingColumns.Select(column => $"Missing required column '{column}'."));

            var description = GetValue(values, header, "description").Trim();
            if (string.IsNullOrWhiteSpace(description))
            {
                errors.Add("Description is required.");
            }

            var amount = TryParseDecimal(GetValue(values, header, "amount"));
            if (amount is null or <= 0)
            {
                errors.Add("Amount must be greater than zero.");
            }

            var type = TryParseEnum<TransactionType>(GetValue(values, header, "type"));
            if (type is null)
            {
                errors.Add("Type must be Income or Expense.");
            }

            var date = TryParseDate(GetValue(values, header, "date"));
            if (date is null)
            {
                errors.Add("Date must use yyyy-MM-dd.");
            }

            var dueDateValue = GetValue(values, header, "dueDate");
            var dueDate = TryParseOptionalDate(dueDateValue);
            if (!string.IsNullOrWhiteSpace(dueDateValue) && dueDate is null)
            {
                errors.Add("Due date must use yyyy-MM-dd.");
            }

            var paymentDateValue = GetValue(values, header, "paymentDate");
            var paymentDate = TryParseOptionalDate(paymentDateValue);
            if (!string.IsNullOrWhiteSpace(paymentDateValue) && paymentDate is null)
            {
                errors.Add("Payment date must use yyyy-MM-dd.");
            }

            var isPaidValue = GetValue(values, header, "isPaid");
            var parsedIsPaid = TryParseOptionalBool(isPaidValue);
            if (!string.IsNullOrWhiteSpace(isPaidValue) && parsedIsPaid is null)
            {
                errors.Add("IsPaid must be true or false.");
            }

            var isPaid = parsedIsPaid ?? false;

            if (isPaid && paymentDate is null)
            {
                errors.Add("Payment date is required when row is paid.");
            }

            if (!isPaid && paymentDate is not null)
            {
                errors.Add("Payment date must be empty when row is pending.");
            }

            var accountId = TryParseGuid(GetValue(values, header, "accountId"));
            if (accountId is null ||
                await _accountRepository.GetByIdAsync(userId, accountId.Value, cancellationToken) is null)
            {
                errors.Add("Account not found.");
            }

            var categoryIdValue = GetValue(values, header, "categoryId");
            var categoryId = TryParseGuid(categoryIdValue);
            var category = categoryId is null
                ? null
                : await _categoryRepository.GetByIdAsync(userId, categoryId.Value, cancellationToken);

            if (category is null &&
                string.IsNullOrWhiteSpace(categoryIdValue) &&
                type.HasValue &&
                !string.IsNullOrWhiteSpace(description))
            {
                var matchingRule = await _categoryKeywordRuleRepository.FindMatchAsync(
                    userId,
                    description,
                    type.Value,
                    cancellationToken);

                category = matchingRule?.Category;
                categoryId = matchingRule?.CategoryId;
            }

            if (category is null)
            {
                errors.Add("Category not found.");
            }
            else if (type.HasValue &&
                     category.Type != CategoryType.Both &&
                     (type == TransactionType.Income && category.Type != CategoryType.Income ||
                      type == TransactionType.Expense && category.Type != CategoryType.Expense))
            {
                errors.Add("Category type is incompatible with transaction type.");
            }

            rows.Add(new CsvImportRowPreviewDto(
                index + 1,
                description,
                amount,
                type,
                date,
                accountId,
                categoryId,
                dueDate,
                isPaid,
                paymentDate,
                errors));
        }

        return rows;
    }

    private static List<List<string>> ParseExcel(string contentBase64, string? worksheetName)
    {
        var bytes = Convert.FromBase64String(contentBase64);
        using var stream = new MemoryStream(bytes);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

        var sharedStrings = ReadSharedStrings(archive);
        var sheetPath = ResolveWorksheetPath(archive, worksheetName);
        var sheetEntry = archive.GetEntry(sheetPath)
            ?? throw new InvalidOperationException($"Worksheet '{sheetPath}' was not found.");

        using var sheetStream = sheetEntry.Open();
        var document = XDocument.Load(sheetStream);
        XNamespace main = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

        var rows = new List<List<string>>();
        foreach (var rowElement in document.Descendants(main + "row"))
        {
            var values = new List<string>();
            foreach (var cellElement in rowElement.Elements(main + "c"))
            {
                var columnIndex = GetColumnIndex((string?)cellElement.Attribute("r"));
                while (values.Count < columnIndex)
                {
                    values.Add(string.Empty);
                }

                values.Add(ReadCellValue(cellElement, sharedStrings, main));
            }

            if (values.Any(value => !string.IsNullOrWhiteSpace(value)))
            {
                rows.Add(values);
            }
        }

        return rows;
    }

    private static IReadOnlyList<string> ReadSharedStrings(ZipArchive archive)
    {
        var entry = archive.GetEntry("xl/sharedStrings.xml");
        if (entry is null)
        {
            return Array.Empty<string>();
        }

        using var stream = entry.Open();
        var document = XDocument.Load(stream);
        XNamespace main = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

        return document
            .Descendants(main + "si")
            .Select(item => string.Concat(item.Descendants(main + "t").Select(text => text.Value)))
            .ToList();
    }

    private static string ResolveWorksheetPath(ZipArchive archive, string? worksheetName)
    {
        var workbookEntry = archive.GetEntry("xl/workbook.xml")
            ?? throw new InvalidOperationException("Excel workbook.xml was not found.");
        var relationshipsEntry = archive.GetEntry("xl/_rels/workbook.xml.rels")
            ?? throw new InvalidOperationException("Excel workbook relationships were not found.");

        using var workbookStream = workbookEntry.Open();
        using var relationshipsStream = relationshipsEntry.Open();
        var workbook = XDocument.Load(workbookStream);
        var relationships = XDocument.Load(relationshipsStream);

        XNamespace main = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        XNamespace rel = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        XNamespace packageRel = "http://schemas.openxmlformats.org/package/2006/relationships";

        var sheet = workbook
            .Descendants(main + "sheet")
            .FirstOrDefault(candidate =>
                string.IsNullOrWhiteSpace(worksheetName) ||
                string.Equals((string?)candidate.Attribute("name"), worksheetName, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException("Requested worksheet was not found.");

        var relationshipId = (string?)sheet.Attribute(rel + "id")
            ?? throw new InvalidOperationException("Worksheet relationship id was not found.");
        var target = relationships
            .Descendants(packageRel + "Relationship")
            .FirstOrDefault(relationship => (string?)relationship.Attribute("Id") == relationshipId)
            ?.Attribute("Target")
            ?.Value
            ?? throw new InvalidOperationException("Worksheet target was not found.");

        return target.StartsWith("xl/", StringComparison.OrdinalIgnoreCase)
            ? target
            : $"xl/{target.TrimStart('/')}";
    }

    private static int GetColumnIndex(string? cellReference)
    {
        if (string.IsNullOrWhiteSpace(cellReference))
        {
            return 0;
        }

        var index = 0;
        foreach (var current in cellReference.TakeWhile(char.IsLetter))
        {
            index *= 26;
            index += char.ToUpperInvariant(current) - 'A' + 1;
        }

        return Math.Max(0, index - 1);
    }

    private static string ReadCellValue(
        XElement cellElement,
        IReadOnlyList<string> sharedStrings,
        XNamespace main)
    {
        var cellType = (string?)cellElement.Attribute("t");
        if (cellType == "inlineStr")
        {
            return string.Concat(cellElement.Descendants(main + "t").Select(text => text.Value));
        }

        var value = cellElement.Element(main + "v")?.Value ?? string.Empty;
        if (cellType == "s" &&
            int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var sharedStringIndex) &&
            sharedStringIndex >= 0 &&
            sharedStringIndex < sharedStrings.Count)
        {
            return sharedStrings[sharedStringIndex];
        }

        return value;
    }

    private static CsvImportPreviewDto BuildPreview(IReadOnlyList<CsvImportRowPreviewDto> rows)
    {
        var validRows = rows.Count(row => row.Errors.Count == 0);
        return new CsvImportPreviewDto(rows.Count, validRows, rows.Count - validRows, rows);
    }

    private static List<List<string>> ParseCsv(string content)
    {
        var rows = new List<List<string>>();
        var row = new List<string>();
        var field = new StringBuilder();
        var inQuotes = false;

        for (var index = 0; index < content.Length; index++)
        {
            var current = content[index];
            if (current == '"')
            {
                if (inQuotes && index + 1 < content.Length && content[index + 1] == '"')
                {
                    field.Append('"');
                    index++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (current == ',' && !inQuotes)
            {
                row.Add(field.ToString());
                field.Clear();
            }
            else if ((current == '\n' || current == '\r') && !inQuotes)
            {
                if (current == '\r' && index + 1 < content.Length && content[index + 1] == '\n')
                {
                    index++;
                }

                row.Add(field.ToString());
                field.Clear();
                if (row.Any(value => !string.IsNullOrWhiteSpace(value)))
                {
                    rows.Add(row);
                }
                row = new List<string>();
            }
            else
            {
                field.Append(current);
            }
        }

        row.Add(field.ToString());
        if (row.Any(value => !string.IsNullOrWhiteSpace(value)))
        {
            rows.Add(row);
        }

        return rows;
    }

    private static string GetValue(IReadOnlyList<string> values, Dictionary<string, int> header, string column)
    {
        return header.TryGetValue(column, out var index) && index < values.Count ? values[index] : string.Empty;
    }

    private static decimal? TryParseDecimal(string value)
    {
        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }

    private static DateOnly? TryParseDate(string value)
    {
        return DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
            ? parsed
            : null;
    }

    private static DateOnly? TryParseOptionalDate(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : TryParseDate(value);
    }

    private static bool? TryParseOptionalBool(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return bool.TryParse(value, out var parsed) ? parsed : null;
    }

    private static Guid? TryParseGuid(string value)
    {
        return Guid.TryParse(value, out var parsed) ? parsed : null;
    }

    private static TEnum? TryParseEnum<TEnum>(string value)
        where TEnum : struct
    {
        return Enum.TryParse<TEnum>(value, true, out var parsed) ? parsed : null;
    }

    private static ImportBatchDto MapToDto(ImportBatch batch)
    {
        return new ImportBatchDto(
            batch.Id,
            batch.FileName,
            batch.FileType,
            batch.ImportedAt,
            batch.TotalRows,
            batch.SuccessRows,
            batch.FailedRows,
            batch.Status);
    }
}
