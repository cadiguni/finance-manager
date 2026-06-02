using FinTrack.Application.Recurring;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.Api.Controllers;

[ApiController]
[Route("api/recurring-rules")]
public sealed class RecurringRulesController : ControllerBase
{
    private static readonly Guid DemoUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private readonly IRecurringRuleService _recurringRuleService;

    public RecurringRulesController(IRecurringRuleService recurringRuleService)
    {
        _recurringRuleService = recurringRuleService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RecurringRuleDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var rules = await _recurringRuleService.GetAllAsync(DemoUserId, cancellationToken);
        return Ok(rules);
    }

    [HttpPost]
    public async Task<ActionResult<RecurringRuleDto>> Create(
        CreateRecurringRuleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _recurringRuleService.CreateAsync(DemoUserId, request, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error });
        }

        return CreatedAtAction(nameof(GetAll), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPost("generate")]
    public async Task<ActionResult<GenerateRecurringTransactionsResult>> Generate(
        GenerateRecurringTransactionsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _recurringRuleService.GenerateAsync(DemoUserId, request, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error });
        }

        return Ok(result.Value);
    }
}
