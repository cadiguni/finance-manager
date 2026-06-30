using FinTrack.Application.Categories;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.Api.Controllers;

[ApiController]
[Route("api/categories")]
public sealed class CategoriesController : ControllerBase
{
    private static readonly Guid DemoUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetAll(CancellationToken cancellationToken)
    {
        var categories = await _categoryService.GetAllAsync(DemoUserId, cancellationToken);

        return Ok(categories);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CategoryDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var category = await _categoryService.GetByIdAsync(DemoUserId, id, cancellationToken);

        return category is null ? NotFound() : Ok(category);
    }

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(
        CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _categoryService.CreateAsync(DemoUserId, request, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _categoryService.UpdateAsync(DemoUserId, id, request, cancellationToken);
        if (!result.IsSuccess)
        {
            return result.Error == "Category not found."
                ? NotFound()
                : BadRequest(new { message = result.Error });
        }

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _categoryService.DeleteAsync(DemoUserId, id, cancellationToken);
        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                "Category not found." => NotFound(),
                "Category has subcategories and cannot be deleted." => Conflict(new { message = result.Error }),
                "Não é possível excluir esta categoria porque ela está sendo usada em transações, recorrências ou regras de categorização." =>
                    Conflict(new { message = result.Error }),
                _ => BadRequest(new { message = result.Error })
            };
        }

        return NoContent();
    }
}
