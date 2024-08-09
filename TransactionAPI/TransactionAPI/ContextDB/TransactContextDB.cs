using Microsoft.EntityFrameworkCore;
using System.Transactions;
using TransactionAPI.Models;

namespace TransactionAPI.ContextDB
{
    public class TransactContextDB : DbContext
    {
        // i add this paramaters if we want to change path config or connectionString
        private readonly string pathConfig = "appsettings.json";
        private readonly string connectionString = "DefaultConnection";

        public DbSet<TransactionModel> Transactions { get; set; }
        public DbSet<GeneralTimeModel> GeneralTimes { get; set; }

        public TransactContextDB() { }
        public TransactContextDB(DbContextOptions<TransactContextDB> options) : base(options) { }
        public TransactContextDB(string newPathConfig, string newConnectionString)
        {
            this.pathConfig = newPathConfig;
            this.connectionString = newConnectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile(pathConfig)
                    .Build();

                optionsBuilder.UseSqlServer(configuration.GetConnectionString(connectionString));
            }
        }
    }
}
