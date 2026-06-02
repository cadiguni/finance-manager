using FinTrack.Application.Installments;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.Api.Controllers;

[ApiController]
[Route("api/installments")]
public sealed class InstallmentsController : ControllerBase
{
    private static readonly Guid DemoUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private readonly IInstallmentService _installmentService;

    public InstallmentsController(IInstallmentService installmentService)
    {
        _installmentService = installmentService;
    }

    [HttpPost]
    public async Task<ActionResult<InstallmentGroupDto>> CreatePurchase(
        CreateInstallmentPurchaseRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _installmentService.CreatePurchaseAsync(DemoUserId, request, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error });
        }

        return CreatedAtAction(nameof(CreatePurchase), new { id = result.Value!.Id }, result.Value);
    }
}
