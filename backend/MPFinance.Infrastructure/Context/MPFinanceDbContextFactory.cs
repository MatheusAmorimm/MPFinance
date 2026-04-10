using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MPFinance.Infrastructure.Context;

public class MPFinanceDbContextFactory : IDesignTimeDbContextFactory<MPFinanceDbContext>
{
    public MPFinanceDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

        if (string.IsNullOrEmpty(connectionString))
        {
            // Fallback: lê o .env manualmente procurando na árvore de diretórios
            var envPath = FindEnvFile(Directory.GetCurrentDirectory());
            if (envPath != null)
            {
                foreach (var line in File.ReadAllLines(envPath))
                {
                    var parts = line.Split('=', 2);
                    if (parts.Length == 2 && parts[0].Trim() == "DB_CONNECTION_STRING")
                    {
                        connectionString = parts[1].Trim().Trim('"');
                        break;
                    }
                }
            }
        }

        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("DB_CONNECTION_STRING não encontrada. Defina a variável de ambiente ou crie o arquivo .env na raiz do projeto.");

        var optionsBuilder = new DbContextOptionsBuilder<MPFinanceDbContext>();
        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

        return new MPFinanceDbContext(optionsBuilder.Options);
    }

    private static string? FindEnvFile(string startPath)
    {
        var dir = new DirectoryInfo(startPath);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, ".env");
            if (File.Exists(candidate)) return candidate;
            dir = dir.Parent;
        }
        return null;
    }
}
