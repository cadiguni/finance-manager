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

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ImportBatchDto>>> GetHistory(
        CancellationToken cancellationToken)
    {
        var history = await _importService.GetHistoryAsync(DemoUserId, cancellationToken);
        return Ok(history);
    }
}
