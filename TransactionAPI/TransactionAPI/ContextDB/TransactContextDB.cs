using Microsoft.EntityFrameworkCore;
using System.Transactions;
using TransactionAPI.Models;

namespace TransactionAPI.ContextDB
{
    public class TransactContextDB : DbContext
    {
        private readonly string pathConfig = "appsettings.json";
        private readonly string connectionString = "DefaultConnection";

        public DbSet<TransactionModel> Transactions { get; set; }
        public DbSet<test> test { get; set; }

        public TransactContextDB() { }
        public TransactContextDB(DbContextOptions<TransactContextDB> options) : base(options) { }

        //If you want to change your connectionString and path config
        public TransactContextDB(string newPathConfig, string newConnectionString)
        {
            this.pathConfig = newPathConfig;
            this.connectionString = newConnectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //if (!optionsBuilder.IsConfigured)
            //{
            //    IConfigurationRoot configuration = new ConfigurationBuilder()
            //        .SetBasePath(Directory.GetCurrentDirectory())
            //        .AddJsonFile(pathConfig)
            //        .Build();

            //    optionsBuilder.UseSqlServer(configuration.GetConnectionString(connectionString), sqlOptions =>
            //    {
            //        sqlOptions.EnableRetryOnFailure(
            //            maxRetryCount: 5,
            //            maxRetryDelay: TimeSpan.FromSeconds(30),
            //            errorNumbersToAdd: null);
            //    });
            //}
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
