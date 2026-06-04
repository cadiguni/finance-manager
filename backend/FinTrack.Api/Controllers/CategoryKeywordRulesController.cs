using FinTrack.Application.Categorization;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.Api.Controllers;

[ApiController]
[Route("api/category-keyword-rules")]
public sealed class CategoryKeywordRulesController : ControllerBase
{
    private static readonly Guid DemoUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private readonly ICategoryKeywordRuleService _service;

    public CategoryKeywordRulesController(ICategoryKeywordRuleService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryKeywordRuleDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var rules = await _service.GetAllAsync(DemoUserId, cancellationToken);
        return Ok(rules);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CategoryKeywordRuleDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var rule = await _service.GetByIdAsync(DemoUserId, id, cancellationToken);
        return rule is null ? NotFound() : Ok(rule);
    }

    [HttpPost]
    public async Task<ActionResult<CategoryKeywordRuleDto>> Create(
        CreateCategoryKeywordRuleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(DemoUserId, request, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateCategoryKeywordRuleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.UpdateAsync(DemoUserId, id, request, cancellationToken);
        if (!result.IsSuccess)
        {
            return result.Error == "Keyword rule not found."
                ? NotFound()
                : BadRequest(new { message = result.Error });
        }

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.DeleteAsync(DemoUserId, id, cancellationToken);
        if (!result.IsSuccess)
        {
            return result.Error == "Keyword rule not found."
                ? NotFound()
                : BadRequest(new { message = result.Error });
        }

        return NoContent();
    }
}
