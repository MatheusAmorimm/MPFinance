using MPFinance.Infrastructure.Context;
using MPFinance.Infrastructure.Repositories;
using MPFinance.Infrastructure.Services;
using MPFinance.Domain.Interfaces;
using MPFinance.Domain.Entities;
using MPFinance.Domain.Enums;
using MPFinance.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DotNetEnv;
using MPFinance.Infrastructure.Security;

Env.TraversePath().Load();
var builder = WebApplication.CreateBuilder(args);
var key = Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET")!);

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
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()));

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
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

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
    if (await context.Categories.AnyAsync()) return;

    // ── Receitas (ordem alfabética) ───────────────────────────────────────────
    await context.Categories.AddRangeAsync(
        new Category("Bolsa de Estudos",          TransactionType.Income),
        new Category("Cashback",                  TransactionType.Income),
        new Category("Freelance & Projetos",      TransactionType.Income),
        new Category("Mesada & Ajuda Familiar",   TransactionType.Income),
        new Category("Pix de Presente",           TransactionType.Income),
        new Category("Pró-labore / PJ",           TransactionType.Income),
        new Category("Reembolso & Restituição",   TransactionType.Income),
        new Category("Rendimentos",               TransactionType.Income),
        new Category("Salário",                   TransactionType.Income),
        new Category("Vendas (Enjoei / OLX)",     TransactionType.Income),

        // ── Despesas (ordem alfabética) ───────────────────────────────────────
        new Category("Aluguel & Moradia",         TransactionType.Expense),
        new Category("Contas de Casa",            TransactionType.Expense),
        new Category("Cursos & Certificações",    TransactionType.Expense),
        new Category("Delivery & Restaurantes",   TransactionType.Expense),
        new Category("Educação & Faculdade",      TransactionType.Expense),
        new Category("Empréstimo & Parcelas",     TransactionType.Expense),
        new Category("Games & Apps",              TransactionType.Expense),
        new Category("Hobby & Esportes",          TransactionType.Expense),
        new Category("Internet & Celular",        TransactionType.Expense),
        new Category("Investimentos & Metas",     TransactionType.Expense),
        new Category("Rolês & Festas",            TransactionType.Expense),
        new Category("Roupas & Acessórios",       TransactionType.Expense),
        new Category("Saúde & Farmácia",          TransactionType.Expense),
        new Category("Streaming & Assinaturas",   TransactionType.Expense),
        new Category("Supermercado",              TransactionType.Expense),
        new Category("Transporte Público",        TransactionType.Expense),
        new Category("Uber & Transporte por App", TransactionType.Expense)
    );

    await context.SaveChangesAsync();
    Console.WriteLine("Categorias padrão criadas com sucesso!");
}
