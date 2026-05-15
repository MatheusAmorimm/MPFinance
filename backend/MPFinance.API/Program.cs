using MediatR;
using System.Threading.RateLimiting;
using MPFinance.Infrastructure.Context;
using MPFinance.Infrastructure.Repositories;
using MPFinance.Infrastructure.Services;
using MPFinance.Domain.Interfaces;
using MPFinance.Domain.Entities;
using MPFinance.Domain.Enums;
using MPFinance.Application.Services;
using MPFinance.Application.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DotNetEnv;
using MPFinance.Infrastructure.Security;

Env.TraversePath().Load();
var builder = WebApplication.CreateBuilder(args);

var jwtSettings = new JwtSettings(
    Secret:   Environment.GetEnvironmentVariable("JWT_SECRET")!,
    Issuer:   Environment.GetEnvironmentVariable("JWT_ISSUER")!,
    Audience: Environment.GetEnvironmentVariable("JWT_AUDIENCE")!
);
var key = Encoding.ASCII.GetBytes(jwtSettings.Secret);

// 1. Configuração do DbContext
builder.Services.AddDbContext<MPFinanceDbContext>();

// 2. Registro dos Repositórios (Scoped - uma instância por request HTTP)
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IGoalRepository, GoalRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IFixedTransactionRepository, FixedTransactionRepository>();

// 3. Registro do Facade (Pattern Estrutural)
builder.Services.AddScoped<FinancialFacade>();

// 4. Registro do MediatR
builder.Services.AddMediatR(typeof(FinancialFacade).Assembly);

// 5. Rate Limiting por IP (endpoints de autenticação)
builder.Services.AddRateLimiter(options =>
{
    static FixedWindowRateLimiterOptions Window(int limit) => new()
    {
        PermitLimit            = limit,
        Window                 = TimeSpan.FromMinutes(1),
        QueueProcessingOrder   = QueueProcessingOrder.OldestFirst,
        QueueLimit             = 0
    };

    // Login, register, resend-verification: 5 tentativas/min por IP
    options.AddPolicy("auth-sensitive", ctx =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => Window(5)));

    // Verify-email: 10 tentativas/min por IP (OTP pode exigir retry)
    options.AddPolicy("auth-verify", ctx =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => Window(10)));

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey         = new SymmetricSecurityKey(key),
        ValidateIssuer           = true,
        ValidIssuer              = jwtSettings.Issuer,
        ValidateAudience         = true,
        ValidAudience            = jwtSettings.Audience
    };
});

builder.Services.AddSingleton(jwtSettings);
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<IPasswordHasher, BCryptHasher>();
builder.Services.AddScoped<IEmailVerificationRepository, EmailVerificationRepository>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();

var app = builder.Build();

// Ensure wwwroot/uploads/goals directory exists
var uploadsPath = Path.Combine(app.Environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "goals");
Directory.CreateDirectory(uploadsPath);

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<MPFinanceDbContext>();
        context.Database.Migrate();
        Console.WriteLine("Banco de dados atualizado com sucesso!");
        await SeedCategoriesAsync(context);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro crítico ao migrar o banco: {ex.Message}");
    }
}

app.UseRateLimiter();
app.UseStaticFiles();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MPFinance API V1");
    c.RoutePrefix = "swagger";
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// ─── Seed inicial de categorias ───────────────────────────────────────────────
static async Task SeedCategoriesAsync(MPFinanceDbContext context)
{
    var canonical = new List<Category>
    {
        // ── Receitas (ordem alfabética) ───────────────────────────────────────
        new("Bolsa de Estudos",          TransactionType.Income),
        new("Cashback",                  TransactionType.Income),
        new("Freelance & Projetos",      TransactionType.Income),
        new("Mesada & Ajuda Familiar",   TransactionType.Income),
        new("Pix de Presente",           TransactionType.Income),
        new("Pró-labore / PJ",           TransactionType.Income),
        new("Reembolso & Restituição",   TransactionType.Income),
        new("Rendimentos",               TransactionType.Income),
        new("Salário",                   TransactionType.Income),
        new("Vendas (Enjoei / OLX)",     TransactionType.Income),

        // ── Despesas (ordem alfabética) ───────────────────────────────────────
        new("Aluguel & Moradia",         TransactionType.Expense),
        new("Condomínio",               TransactionType.Expense),
        new("Conta de Água",            TransactionType.Expense),
        new("Conta de Luz",             TransactionType.Expense),
        new("Cursos & Certificações",   TransactionType.Expense),
        new("Delivery & Restaurantes",  TransactionType.Expense),
        new("Educação & Faculdade",     TransactionType.Expense),
        new("Empréstimo & Parcelas",    TransactionType.Expense),
        new("Games & Apps",             TransactionType.Expense),
        new("Gás de Cozinha",           TransactionType.Expense),
        new("Hobby & Esportes",         TransactionType.Expense),
        new("Internet & Celular",       TransactionType.Expense),
        new("Rolês & Festas",           TransactionType.Expense),
        new("Roupas & Acessórios",       TransactionType.Expense),
        new("Saúde & Farmácia",          TransactionType.Expense),
        new("Streaming & Assinaturas",   TransactionType.Expense),
        new("Supermercado",              TransactionType.Expense),
        new("Transporte Público",        TransactionType.Expense),
        new("Uber & Transporte por App", TransactionType.Expense),
    };

    var canonicalNames = canonical.Select(c => c.Name).ToHashSet();

    // Remove categorias fora da lista canônica que não possuem transações vinculadas
    var usedCategoryIds = await context.Transactions
        .Select(t => t.CategoryId)
        .Distinct()
        .ToListAsync();

    var toRemove = await context.Categories
        .Where(c => !canonicalNames.Contains(c.Name) && !usedCategoryIds.Contains(c.Id))
        .ToListAsync();

    if (toRemove.Count > 0)
    {
        context.Categories.RemoveRange(toRemove);
        await context.SaveChangesAsync();
        Console.WriteLine($"{toRemove.Count} categoria(s) obsoleta(s) removida(s).");
    }

    // Insere categorias da lista canônica que ainda não existem
    var existing = (await context.Categories.Select(c => c.Name).ToListAsync()).ToHashSet();
    var toAdd = canonical.Where(c => !existing.Contains(c.Name)).ToList();

    if (toAdd.Count > 0)
    {
        await context.Categories.AddRangeAsync(toAdd);
        await context.SaveChangesAsync();
        Console.WriteLine($"{toAdd.Count} categoria(s) adicionada(s).");
    }
}
