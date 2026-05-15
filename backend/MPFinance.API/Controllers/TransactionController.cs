using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MPFinance.Application.DTOs;
using MPFinance.Application.Services;
using System.Security.Claims;

namespace MPFinance.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionController : ControllerBase
{
    private readonly FinancialFacade _financialFacade;

    public TransactionController(FinancialFacade financialFacade)
    {
        _financialFacade = financialFacade;
    }

    [HttpGet]
    public async Task<IActionResult> GetByMonth([FromQuery] int? month, [FromQuery] int? year)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var now = DateTime.UtcNow;
        var result = await _financialFacade.GetUserTransactionsAsync(
            userId,
            month ?? now.Month,
            year  ?? now.Year
        );

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTransactionRequest request)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        try
        {
            await _financialFacade.CreateTransactionWithImpactAsync(
                userId,
                request.CategoryId,
                request.Amount,
                request.Description,
                request.Date,
                request.GoalId
            );

            return Ok(new { message = "Lançamento realizado com sucesso!" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTransactionRequest request)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var updated = await _financialFacade.UpdateTransactionAsync(userId, id, request.Amount, request.Description, request.Date);
        return updated ? Ok(new { message = "Lançamento atualizado." }) : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var deleted = await _financialFacade.DeleteTransactionAsync(userId, id);
        return deleted ? NoContent() : NotFound();
    }

    // ─── Helper ───────────────────────────────────────────────────────────────

    private bool TryGetUserId(out Guid userId)
    {
        userId = Guid.Empty;
        var raw = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(raw, out userId);
    }
}

public record CreateTransactionRequest(
    [Required] Guid                                                    CategoryId,
    [Range(0.01, 1_000_000.00)] decimal                               Amount,
    [Required, StringLength(255, MinimumLength = 1)] string           Description,
    [Required] DateTime                                                Date,
    Guid?                                                              GoalId
);

public record UpdateTransactionRequest(
    [Range(0.01, 1_000_000.00)] decimal                     Amount,
    [Required, StringLength(255, MinimumLength = 1)] string  Description,
    [Required] DateTime                                      Date
);
