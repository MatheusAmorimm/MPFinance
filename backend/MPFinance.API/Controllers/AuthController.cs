using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
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
    private readonly IMediator _mediator;
    private readonly IUserRepository _userRepository;
    private readonly IEmailVerificationRepository _verificationRepo;

    public AuthController(
        IPasswordHasher passwordHasher,
        TokenService tokenService,
        IMediator mediator,
        IUserRepository userRepository,
        IEmailVerificationRepository verificationRepo)
    {
        _passwordHasher   = passwordHasher;
        _tokenService     = tokenService;
        _mediator         = mediator;
        _userRepository   = userRepository;
        _verificationRepo = verificationRepo;
    }

    [HttpPost("register")]
    [EnableRateLimiting("auth-sensitive")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var existing = await _userRepository.GetByEmailAsync(request.Email);
        if (existing != null)
            return Conflict(new { message = "Este e-mail já está em uso." });

        var passwordHash = _passwordHasher.Hash(request.Password);
        var newUser = new User(request.Name, request.Email, passwordHash);

        await _userRepository.AddAsync(newUser);
        await _userRepository.SaveChangesAsync();

        await _mediator.Publish(new UserRegisteredEvent(newUser));

        return Ok(new { message = "Conta criada com sucesso!" });
    }

    [HttpPost("login")]
    [EnableRateLimiting("auth-sensitive")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "Senha ou E-mail incorretos." });

        if (!user.IsVerified)
            return StatusCode(403, new { message = "Verifique seu e-mail antes de fazer login." });

        var token = _tokenService.GenerateToken(user);
        return Ok(new { token });
    }

    [HttpPost("resend-verification")]
    [EnableRateLimiting("auth-sensitive")]
    public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
            return NotFound(new { message = "Usuário não encontrado." });

        if (user.IsVerified)
            return BadRequest(new { message = "Este e-mail já foi verificado." });

        await _mediator.Publish(new UserRegisteredEvent(user));

        return Ok(new { message = "Novo código enviado!" });
    }

    [HttpPost("verify-email")]
    [EnableRateLimiting("auth-verify")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
            return NotFound(new { message = "Usuário não encontrado." });

        var validCode = await _verificationRepo.GetValidCodeAsync(user.Id, request.Code);
        if (validCode == null)
            return BadRequest(new { message = "Código inválido ou expirado." });

        user.VerifyEmail();
        _userRepository.Update(user);

        await _verificationRepo.DeleteAllForUserAsync(user.Id);
        await _userRepository.SaveChangesAsync();

        var token = _tokenService.GenerateToken(user);
        return Ok(new { message = "E-mail verificado com sucesso!", token });
    }
}

public record RegisterRequest(
    [Required, StringLength(100, MinimumLength = 2)] string Name,
    [Required, EmailAddress]                         string Email,
    [Required, StringLength(100, MinimumLength = 8)] string Password
);

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required]               string Password
);

public record VerifyEmailRequest(
    [Required, EmailAddress]                      string Email,
    [Required, StringLength(6, MinimumLength = 6)] string Code
);

public record ResendVerificationRequest(
    [Required, EmailAddress] string Email
);
