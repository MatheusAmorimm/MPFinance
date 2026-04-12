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

    // ─── Helper ───────────────────────────────────────────────────────────────

    private bool TryGetUserId(out Guid userId)
    {
        userId = Guid.Empty;
        var raw = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(raw, out userId);
    }
}

public record CreateTransactionRequest(
    Guid CategoryId,
    decimal Amount,
    string Description,
    DateTime Date,
    Guid? GoalId
);
