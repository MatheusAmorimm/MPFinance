namespace MPFinance.Application.Settings;

public record JwtSettings(string Secret, string Issuer, string Audience);
