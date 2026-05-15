using System.ComponentModel.DataAnnotations;

namespace MPFinance.Application.DTOs;

public record ChatMessageItem(string Role, string Content);

public record ChatRequest(
    [Required][MaxLength(800)] string Message,
    bool ShareFinancialData,
    int Month,
    int Year,
    List<ChatMessageItem>? History
);

public record ChatResponse(string Reply);
