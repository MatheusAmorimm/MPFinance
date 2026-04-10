using Microsoft.AspNetCore.Mvc;
using MPFinance.Application.Services;
using MPFinance.Domain.Interfaces;
using MPFinance.Domain.Entities;
using MediatR;
using MPFinance.Domain.Events;

namespace MPFinance.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IPasswordHasher _passwordHasher;
    private readonly TokenService _tokenService;
    private readonly IMediator _mediator; // Para o Observer de boas-vindas

    public AuthController(IPasswordHasher passwordHasher, TokenService tokenService, IMediator mediator)
    {
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // 1. Hash da senha (Strategy)
        var passwordHash = _passwordHasher.Hash(request.Password);
        
        // 2. Criação do usuário (Aqui você usaria seu repositório)
        var newUser = new User(request.Name, request.Email, passwordHash);
        
        // [Simulação: Salvar no banco via Repo]
        
        // 3. Dispara Evento (Observer)
        await _mediator.Publish(new UserRegisteredEvent(newUser));

        return Ok(new { message = "Usuário registrado com sucesso!" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        return Ok(new { token = "JWT_TOKEN_GERADO_AQUI" });
    }
}

public record RegisterRequest(string Name, string Email, string Password);
public record LoginRequest(string Email, string Password);