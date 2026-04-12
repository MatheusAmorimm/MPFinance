using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MPFinance.Application.Services;
using System.Security.Claims;

namespace MPFinance.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HomeController : ControllerBase
{
    private readonly FinancialFacade _financialFacade;

    public HomeController(FinancialFacade financialFacade)
    {
        _financialFacade = financialFacade;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] int? month, [FromQuery] int? year)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out var userId))
            return Unauthorized();

        var now = DateTime.UtcNow;
        var summary = await _financialFacade.GetMonthlySummaryAsync(
            userId,
            month ?? now.Month,
            year ?? now.Year,
            now.Day
        );

        return Ok(summary);
    }
}
