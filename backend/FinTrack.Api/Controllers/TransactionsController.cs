using FinTrack.Application.Transactions;
using FinTrack.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.Api.Controllers;

[ApiController]
[Route("api/transactions")]
public sealed class TransactionsController : ControllerBase
{
    private static readonly Guid DemoUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TransactionDto>>> GetAll(
        [FromQuery] DateOnly? startDate,
        [FromQuery] DateOnly? endDate,
        [FromQuery] Guid? categoryId,
        [FromQuery] Guid? accountId,
        [FromQuery] TransactionType? type,
        [FromQuery] bool? isPaid,
        CancellationToken cancellationToken)
    {
        var filters = new TransactionFilters(startDate, endDate, categoryId, accountId, type, isPaid);
        var transactions = await _transactionService.GetAllAsync(DemoUserId, filters, cancellationToken);

        return Ok(transactions);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TransactionDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var transaction = await _transactionService.GetByIdAsync(DemoUserId, id, cancellationToken);

        return transaction is null ? NotFound() : Ok(transaction);
    }

    [HttpPost]
    public async Task<ActionResult<TransactionDto>> Create(
        CreateTransactionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _transactionService.CreateAsync(DemoUserId, request, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Error });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateTransactionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _transactionService.UpdateAsync(DemoUserId, id, request, cancellationToken);
        if (!result.IsSuccess)
        {
            return result.Error == "Transaction not found."
                ? NotFound()
                : BadRequest(new { message = result.Error });
        }

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _transactionService.DeleteAsync(DemoUserId, id, cancellationToken);
        if (!result.IsSuccess)
        {
            return result.Error == "Transaction not found."
                ? NotFound()
                : BadRequest(new { message = result.Error });
        }

        return NoContent();
    }
}
