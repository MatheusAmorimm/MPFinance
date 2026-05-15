using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MPFinance.Application.DTOs;
using MPFinance.Application.Services;

namespace MPFinance.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController(ChatService chatService) : ControllerBase
{
    private readonly ChatService _chat = chatService;

    [HttpPost]
    public async Task<IActionResult> Ask([FromBody] ChatRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (ChatService.ContainsInjectionAttempt(request.Message))
            return Ok(new ChatResponse("Só posso ajudar com dúvidas sobre o MPFinance e finanças pessoais."));

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        try
        {
            var reply = await _chat.AskAsync(userId, request);
            return Ok(new ChatResponse(reply));
        }
        catch (HttpRequestException ex) when (ex.Message.StartsWith("Limite"))
        {
            return StatusCode(429, new ChatResponse(ex.Message));
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[ChatController] Erro: {ex.Message}");
            return StatusCode(500, new ChatResponse("Serviço de IA temporariamente indisponível. Tente novamente."));
        }
    }
}
