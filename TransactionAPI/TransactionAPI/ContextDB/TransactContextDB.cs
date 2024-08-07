using Microsoft.EntityFrameworkCore;
using System.Transactions;
using TransactionAPI.Models;

namespace TransactionAPI.ContextDB
{
    public class TransactContextDB : DbContext
    {
        static public readonly string pathConfig = "appsettings.json";
        static public readonly string connectionString = "DefaultConnection";

        //public DbSet<TransactionModel> Transactions { get; set; }
        public DbSet<test> test { get; set; }

        public TransactContextDB() { }
        public TransactContextDB(DbContextOptions<TransactContextDB> options) : base(options) { }

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
