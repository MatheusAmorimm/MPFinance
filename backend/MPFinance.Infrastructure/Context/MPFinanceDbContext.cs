using Microsoft.EntityFrameworkCore;
using MPFinance.Infrastructure.Configuration;
using MPFinance.Domain.Entities;

namespace MPFinance.Infrastructure.Context
{
    public class MPFinanceDbContext : DbContext
    {
        // Construtor usado pelo DI container em runtime
        public MPFinanceDbContext() { }

        // Construtor usado pela IDesignTimeDbContextFactory (dotnet ef migrations)
        public MPFinanceDbContext(DbContextOptions<MPFinanceDbContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Só configura via Singleton se não foi configurado via construtor (design-time)
            if (!optionsBuilder.IsConfigured)
            {
                var config = DbConfiguration.Instance;
                optionsBuilder.UseMySql(config.ConnectionString, ServerVersion.AutoDetect(config.ConnectionString));
            }
        }

        // Mapeamento das Tabelas (Baseado na sua imagem)
        public DbSet<User> Users { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<FixedTransaction> FixedTransactions { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Goal> Goals { get; set; }
        public DbSet<PasswordReset> PasswordResets { get; set; }
        public DbSet<EmailVerificationCode> EmailVerificationCodes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Aqui aplicaremos as regras de relacionamentos 1:N da imagem futuramente
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(MPFinanceDbContext).Assembly);
        }
    }
}