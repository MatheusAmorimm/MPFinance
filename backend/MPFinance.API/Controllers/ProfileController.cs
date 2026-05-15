using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MPFinance.Domain.Entities;
using MPFinance.Domain.Interfaces;

namespace MPFinance.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IUserRepository _userRepo;
    private readonly IEmailVerificationRepository _codeRepo;
    private readonly IEmailService _emailService;
    private readonly IPasswordHasher _passwordHasher;

    private const int EmailChangeCooldownDays = 15;
    private const int CodeExpiryMinutes       = 15;

    public ProfileController(
        IUserRepository userRepo,
        IEmailVerificationRepository codeRepo,
        IEmailService emailService,
        IPasswordHasher passwordHasher)
    {
        _userRepo        = userRepo;
        _codeRepo        = codeRepo;
        _emailService    = emailService;
        _passwordHasher  = passwordHasher;
    }

    // ─── GET /api/profile ────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var user = await GetCurrentUserAsync();
        if (user is null) return Unauthorized();

        return Ok(new
        {
            id             = user.Id,
            name           = user.Name,
            email          = user.Email,
            createdAt      = user.CreatedAt,
            emailChangedAt = user.EmailChangedAt,
        });
    }

    // ─── POST /api/profile/change-password/request ───────────────────────────
    [HttpPost("change-password/request")]
    public async Task<IActionResult> RequestPasswordChange()
    {
        var user = await GetCurrentUserAsync();
        if (user is null) return Unauthorized();

        var code = await IssueCodeAsync(user.Id);
        await _emailService.SendPasswordChangeCodeAsync(user.Email, user.Name, code);

        return Ok(new { message = "Código enviado para o seu e-mail." });
    }

    // ─── POST /api/profile/change-password/confirm ───────────────────────────
    [HttpPost("change-password/confirm")]
    public async Task<IActionResult> ConfirmPasswordChange([FromBody] ConfirmPasswordRequest req)
    {
        var user = await GetCurrentUserAsync();
        if (user is null) return Unauthorized();

        var valid = await _codeRepo.GetValidCodeAsync(user.Id, req.Code);
        if (valid is null) return BadRequest(new { message = "Código inválido ou expirado." });

        user.UpdatePassword(_passwordHasher.Hash(req.NewPassword));
        _userRepo.Update(user);

        await _codeRepo.DeleteAllForUserAsync(user.Id);
        await _userRepo.SaveChangesAsync();

        return Ok(new { message = "Senha alterada com sucesso!" });
    }

    // ─── POST /api/profile/change-email/request ──────────────────────────────
    [HttpPost("change-email/request")]
    public async Task<IActionResult> RequestEmailChange()
    {
        var user = await GetCurrentUserAsync();
        if (user is null) return Unauthorized();

        if (user.EmailChangedAt.HasValue)
        {
            var daysSince = (DateTime.UtcNow - user.EmailChangedAt.Value).TotalDays;
            if (daysSince < EmailChangeCooldownDays)
            {
                var daysLeft = (int)Math.Ceiling(EmailChangeCooldownDays - daysSince);
                return BadRequest(new
                {
                    message  = $"Você só pode alterar o e-mail a cada {EmailChangeCooldownDays} dias. Aguarde {daysLeft} dia(s).",
                    daysLeft
                });
            }
        }

        // Clear any pending state from a previous incomplete flow
        user.ClearPendingEmail();
        _userRepo.Update(user);

        var code = await IssueCodeAsync(user.Id);
        await _emailService.SendEmailChangeCodeAsync(user.Email, user.Name, code, isNewEmail: false);
        await _userRepo.SaveChangesAsync();

        return Ok(new { message = "Código enviado para o seu e-mail atual." });
    }

    // ─── POST /api/profile/change-email/submit ────────────────────────────────
    /// <summary>
    /// Validates OTP sent to current email, stores the new email, sends OTP to the new email.
    /// </summary>
    [HttpPost("change-email/submit")]
    public async Task<IActionResult> SubmitNewEmail([FromBody] SubmitNewEmailRequest req)
    {
        var user = await GetCurrentUserAsync();
        if (user is null) return Unauthorized();

        if (!req.NewEmail.Equals(req.ConfirmEmail, StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Os e-mails informados não coincidem." });

        if (req.NewEmail.Equals(user.Email, StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "O novo e-mail não pode ser igual ao atual." });

        var existing = await _userRepo.GetByEmailAsync(req.NewEmail);
        if (existing is not null)
            return Conflict(new { message = "Este e-mail já está em uso por outra conta." });

        var valid = await _codeRepo.GetValidCodeAsync(user.Id, req.Code);
        if (valid is null) return BadRequest(new { message = "Código inválido ou expirado." });

        // Store pending email and send OTP to the new address
        user.StartEmailChange(req.NewEmail);
        _userRepo.Update(user);

        var code = await IssueCodeAsync(user.Id);
        await _emailService.SendEmailChangeCodeAsync(req.NewEmail, user.Name, code, isNewEmail: true);
        await _userRepo.SaveChangesAsync();

        return Ok(new { message = "Código enviado para o novo e-mail." });
    }

    // ─── POST /api/profile/change-email/verify-new ───────────────────────────
    [HttpPost("change-email/verify-new")]
    public async Task<IActionResult> VerifyNewEmail([FromBody] VerifyCodeRequest req)
    {
        var user = await GetCurrentUserAsync();
        if (user is null) return Unauthorized();

        if (string.IsNullOrWhiteSpace(user.PendingEmail))
            return BadRequest(new { message = "Nenhuma alteração de e-mail pendente." });

        var valid = await _codeRepo.GetValidCodeAsync(user.Id, req.Code);
        if (valid is null) return BadRequest(new { message = "Código inválido ou expirado." });

        var newEmail = user.PendingEmail;
        user.CompleteEmailChange();
        _userRepo.Update(user);

        await _codeRepo.DeleteAllForUserAsync(user.Id);
        await _userRepo.SaveChangesAsync();

        return Ok(new { message = "E-mail alterado com sucesso!", newEmail });
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────
    private async Task<User?> GetCurrentUserAsync()
    {
        var raw = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(raw, out var userId)) return null;
        return await _userRepo.GetByIdAsync(userId);
    }

    private async Task<string> IssueCodeAsync(Guid userId)
    {
        await _codeRepo.DeleteAllForUserAsync(userId);
        var code = RandomNumberGenerator.GetInt32(100_000, 1_000_000).ToString();
        var verificationCode = new EmailVerificationCode(userId, code, DateTime.UtcNow.AddMinutes(CodeExpiryMinutes));
        await _codeRepo.AddAsync(verificationCode);
        await _codeRepo.SaveChangesAsync();
        return code;
    }
}

// ─── Request records ──────────────────────────────────────────────────────────
public record ConfirmPasswordRequest(
    [Required, StringLength(6, MinimumLength = 6)]  string Code,
    [Required, StringLength(100, MinimumLength = 8)] string NewPassword
);

public record SubmitNewEmailRequest(
    [Required, StringLength(6, MinimumLength = 6)] string Code,
    [Required, EmailAddress]                       string NewEmail,
    [Required, EmailAddress]                       string ConfirmEmail
);

public record VerifyCodeRequest(
    [Required, StringLength(6, MinimumLength = 6)] string Code
);
