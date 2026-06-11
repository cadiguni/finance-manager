using FinTrack.Application.Common;
using FinTrack.Application.Imports;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.Api.Controllers;

[ApiController]
[Route("api/imports")]
public sealed class ImportsController : ControllerBase
{
    private static readonly Guid DemoUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private readonly IImportService _importService;

    public ImportsController(IImportService importService)
    {
        _importService = importService;
    }

    [HttpPost("csv/preview")]
    public async Task<ActionResult<CsvImportPreviewDto>> PreviewCsv(
        CsvPreviewRequest request,
        CancellationToken cancellationToken)
    {
        var preview = await _importService.PreviewCsvAsync(DemoUserId, request, cancellationToken);
        return Ok(preview);
    }

    [HttpPost("csv/commit")]
    public async Task<ActionResult<ImportBatchDto>> CommitCsv(
        CommitCsvImportRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _importService.CommitCsvAsync(DemoUserId, request, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpPost("excel/preview")]
    public async Task<ActionResult<CsvImportPreviewDto>> PreviewExcel(
        ExcelPreviewRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var preview = await _importService.PreviewExcelAsync(DemoUserId, request, cancellationToken);
            return Ok(preview);
        }
        catch (FormatException)
        {
            return BadRequest(new { message = "Excel content must be a valid base64 string." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("excel/commit")]
    public async Task<ActionResult<ImportBatchDto>> CommitExcel(
        CommitExcelImportRequest request,
        CancellationToken cancellationToken)
    {
        Result<ImportBatchDto> result;
        try
        {
            result = await _importService.CommitExcelAsync(DemoUserId, request, cancellationToken);
        }
        catch (FormatException)
        {
            return BadRequest(new { message = "Excel content must be a valid base64 string." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpPost("card-statement/preview")]
    public async Task<ActionResult<CsvImportPreviewDto>> PreviewCardStatement(
        CardStatementPreviewRequest request,
        CancellationToken cancellationToken)
    {
        var preview = await _importService.PreviewCardStatementAsync(DemoUserId, request, cancellationToken);
        return Ok(preview);
    }

    [HttpPost("card-statement/commit")]
    public async Task<ActionResult<ImportBatchDto>> CommitCardStatement(
        CommitCardStatementImportRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _importService.CommitCardStatementAsync(DemoUserId, request, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ImportBatchDto>>> GetHistory(
        CancellationToken cancellationToken)
    {
        var history = await _importService.GetHistoryAsync(DemoUserId, cancellationToken);
        return Ok(history);
    }
}
