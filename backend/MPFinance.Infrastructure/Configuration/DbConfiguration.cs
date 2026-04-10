namespace MPFinance.Infrastructure.Configuration
{
    public sealed class DbConfiguration
    {
        private static DbConfiguration? _instance;
        private static readonly object _lock = new object();

        public string ConnectionString { get; private set; }

        // O construtor privado garante o Singleton
        private DbConfiguration()
        {
            // Busca a string de conexão das variáveis de ambiente (injetadas via Docker/Env)
            ConnectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") 
                ?? throw new InvalidOperationException("Connection string não encontrada.");
        }

        public static DbConfiguration Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new DbConfiguration();
                    }
                    return _instance;
                }
            }
        }
    }
}