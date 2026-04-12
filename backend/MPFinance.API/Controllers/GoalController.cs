using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MPFinance.Domain.Entities;
using MPFinance.Domain.Interfaces;
using System.Security.Claims;

namespace MPFinance.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GoalController : ControllerBase
{
    private readonly IGoalRepository _goalRepo;
    private readonly IWebHostEnvironment _env;

    public GoalController(IGoalRepository goalRepo, IWebHostEnvironment env)
    {
        _goalRepo = goalRepo;
        _env = env;
    }

    // ─── GET /api/goal ────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetGoals()
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var goals = await _goalRepo.GetByUserIdAsync(userId);

        return Ok(goals.Select(MapGoal));
    }

    // ─── POST /api/goal ───────────────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> CreateGoal([FromBody] CreateGoalRequest req)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var goal = new Goal(userId, req.Title, req.TargetAmount, req.Deadline);
        await _goalRepo.AddAsync(goal);
        await _goalRepo.SaveChangesAsync();

        return CreatedAtAction(nameof(GetGoals), new { id = goal.Id }, MapGoal(goal));
    }

    // ─── PUT /api/goal/{id} ───────────────────────────────────────────────────
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateGoal(Guid id, [FromBody] UpdateGoalRequest req)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var goal = await _goalRepo.GetByIdAsync(id);
        if (goal is null || goal.UserId != userId) return NotFound();

        goal.UpdateDetails(req.Title, req.TargetAmount, req.Deadline);
        _goalRepo.Update(goal);
        await _goalRepo.SaveChangesAsync();

        return Ok(MapGoal(goal));
    }

    // ─── DELETE /api/goal/{id} ────────────────────────────────────────────────
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteGoal(Guid id)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var goal = await _goalRepo.GetByIdAsync(id);
        if (goal is null || goal.UserId != userId) return NotFound();

        // Remove old cover image file if present
        if (!string.IsNullOrEmpty(goal.CoverImageUrl))
            DeleteImageFile(goal.CoverImageUrl);

        _goalRepo.Delete(goal);
        await _goalRepo.SaveChangesAsync();

        return NoContent();
    }

    // ─── POST /api/goal/{id}/cover-image ─────────────────────────────────────
    [HttpPost("{id:guid}/cover-image")]
    public async Task<IActionResult> UploadCoverImage(Guid id, IFormFile file)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var goal = await _goalRepo.GetByIdAsync(id);
        if (goal is null || goal.UserId != userId) return NotFound();

        if (file is null || file.Length == 0)
            return BadRequest("Nenhum arquivo enviado.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext is not (".jpg" or ".jpeg" or ".png" or ".webp"))
            return BadRequest("Formato não suportado. Use JPG, PNG ou WebP.");

        if (file.Length > 5 * 1024 * 1024)
            return BadRequest("A imagem deve ter no máximo 5 MB.");

        // Delete previous image
        if (!string.IsNullOrEmpty(goal.CoverImageUrl))
            DeleteImageFile(goal.CoverImageUrl);

        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "goals");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{id}{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        var url = $"/uploads/goals/{fileName}";
        goal.SetCoverImage(url);
        _goalRepo.Update(goal);
        await _goalRepo.SaveChangesAsync();

        return Ok(new { url });
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────
    private bool TryGetUserId(out Guid userId)
    {
        userId = Guid.Empty;
        var raw = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(raw, out userId);
    }

    private static object MapGoal(Goal g) => new
    {
        id            = g.Id,
        title         = g.Title,
        targetAmount  = g.TargetAmount,
        currentAmount = g.CurrentAmount,
        deadline      = g.Deadline,
        createdAt     = g.CreatedAt,
        coverImageUrl = g.CoverImageUrl,
    };

    private void DeleteImageFile(string relativeUrl)
    {
        var filePath = Path.Combine(_env.WebRootPath, relativeUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (System.IO.File.Exists(filePath))
            System.IO.File.Delete(filePath);
    }
}

// ─── Request records ──────────────────────────────────────────────────────────
public record CreateGoalRequest(string Title, decimal TargetAmount, DateTime Deadline);
public record UpdateGoalRequest(string Title, decimal TargetAmount, DateTime Deadline);
