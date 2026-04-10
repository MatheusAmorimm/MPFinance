using Microsoft.AspNetCore.Mvc;
using MPFinance.Application.Services;

namespace MPFinance.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionController : ControllerBase
{
    private readonly FinancialFacade _financialFacade;

    public TransactionController(FinancialFacade financialFacade)
    {
        _financialFacade = financialFacade;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TransactionRequest request)
    {
        try
        {
            // O Controller apenas recebe os dados e repassa para o Facade
            await _financialFacade.CreateTransactionWithImpactAsync(
                request.UserId,
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
}

// DTO para o Request (Clean Code: isolando o input da API)
public record TransactionRequest(
    Guid UserId, 
    Guid CategoryId, 
    decimal Amount, 
    string Description, 
    DateTime Date, 
    Guid? GoalId
);