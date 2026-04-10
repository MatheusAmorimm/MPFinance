using MPFinance.Infrastructure.Context;
using MPFinance.Infrastructure.Repositories;
using MPFinance.Domain.Interfaces;
using MPFinance.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DotNetEnv;
using MPFinance.Infrastructure.Security;     // Resolve o BCryptHasher

Env.TraversePath().Load();
var builder = WebApplication.CreateBuilder(args);
var key = Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET")!);

// 1. Configuração do DbContext (Usando o Singleton internamente na classe)
builder.Services.AddDbContext<MPFinanceDbContext>();

// 2. Registro dos Repositórios (Scoped - uma instância por request HTTP)
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IGoalRepository, GoalRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// 3. Registro do Facade (Pattern Estrutural)
builder.Services.AddScoped<FinancialFacade>();

// 4. Registro do MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // Essencial para testarmos o MVP

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false; // Setar para true em produção
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

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<MPFinanceDbContext>();
        // Tenta aplicar qualquer migration pendente no banco de dados. 
        // Se o banco não existir, ele cria.
        context.Database.Migrate();
        Console.WriteLine("Banco de dados atualizado com sucesso!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro crítico ao migrar o banco: {ex.Message}");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MPFinance API V1");
        c.RoutePrefix = "swagger"; // Isso garante que seja localhost:5000/swagger
    });
}

//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();