using Dapper;
using Microsoft.Data.SqlClient;
using System.Transactions;
using TransactionAPI.BusinessLogic;
using TransactionAPI.Models;
using TransactionAPI.Services.Interfaces;

namespace TransactionAPI.Services.Implementation
{
    public class TransactionService : ITransactionService
    {
        private readonly string _connectionString;

        public TransactionService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // in this method we get all data from our Transactions table
        public async Task<IEnumerable<TransactionModel>> GetTransactionsAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = "SELECT transaction_id AS TransactionId, name, email, amount, transaction_date AS TransactionDate, client_location AS ClientLocation FROM Transactions";
                return await connection.QueryAsync<TransactionModel>(sql);
            }
        }

        // in this method we add or update our data which we receive from csv file
        public async Task AddOrUpdateTransactionAsync(TransactionModel transaction)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"
                IF EXISTS (SELECT 1 FROM Transactions WHERE transaction_id = @TransactionId)
                BEGIN
                    UPDATE Transactions
                    SET name = @Name,
                        email = @Email,
                        amount = @Amount,
                        transaction_date = @TransactionDate,
                        client_location = @ClientLocation
                    WHERE transaction_id = @TransactionId
                END
                ELSE
                BEGIN
                    INSERT INTO Transactions (transaction_id, name, email, amount, transaction_date, client_location)
                    VALUES (@TransactionId, @Name, @Email, @Amount, @TransactionDate, @ClientLocation)
                END";

                var parameters = new
                {
                    transaction.TransactionId,
                    transaction.Name,
                    transaction.Email,
                    transaction.Amount,
                    transaction.TransactionDate,
                    transaction.ClientLocation
                };

                await connection.ExecuteAsync(sql, parameters);
            }
        }

        // this method for get data from db which are in the certain period and the certain time zone
        public async Task<IEnumerable<TransactionModel>> GetTransactionsInUserTimeZoneAsync(DateTime startDate, DateTime endDate, string userCoordinates)
        {
            // We get the user's time zone based on its coordinates
            string userTimeZone = BusinessClass.GetTimeZoneFromCoordinates(userCoordinates);

            // Convert date range to UTC
            var startDateUtc = BusinessClass.ConvertToGeneralTimeZone(startDate.ToString(), userTimeZone);
            var endDateUtc = BusinessClass.ConvertToGeneralTimeZone(endDate.ToString(), userTimeZone);

            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT t.transaction_id AS TransactionId, 
                           t.name AS Name, 
                           t.email AS Email, 
                           t.amount AS Amount, 
                           t.transaction_date AS TransactionDate, 
                           t.client_location AS ClientLocation 
                    FROM GeneralTimes g
                    INNER JOIN Transactions t ON g.transaction_id = t.transaction_id
                    WHERE g.general_time >= @StartDateUtc AND g.general_time <= @EndDateUtc AND g.timezone = @UserTimeZone";

                var transactions = await connection.QueryAsync<TransactionModel>(sql, new { StartDateUtc = startDateUtc, 
                                                                                            EndDateUtc = endDateUtc, 
                                                                                            UserTimeZone = userTimeZone });

                return transactions;
            }
        }

        // this as previous method (GetTransactionsInUserTimeZoneAsync) but in this case it's for all time zone
        public async Task<IEnumerable<TransactionModel>> GetTransactionsAllTimeZoneAndPeriodTimeAsync(DateTime startDate, DateTime endDate, string userCoordinates)
        {
            // We get the user's time zone based on its coordinates
            string userTimeZone = BusinessClass.GetTimeZoneFromCoordinates(userCoordinates);

            // Convert date range to UTC
            var startDateUtc = BusinessClass.ConvertToGeneralTimeZone(startDate.ToString(), userTimeZone);
            var endDateUtc = BusinessClass.ConvertToGeneralTimeZone(endDate.ToString(), userTimeZone);

            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT t.transaction_id AS TransactionId, 
                           t.name AS Name, 
                           t.email AS Email, 
                           t.amount AS Amount, 
                           t.transaction_date AS TransactionDate, 
                           t.client_location AS ClientLocation 
                    FROM GeneralTimes g
                    INNER JOIN Transactions t ON g.transaction_id = t.transaction_id
                    WHERE g.general_time >= @StartDateUtc AND g.general_time <= @EndDateUtc";

                var transactions = await connection.QueryAsync<TransactionModel>(sql, new
                {
                    StartDateUtc = startDateUtc,
                    EndDateUtc = endDateUtc,
                    UserTimeZone = userTimeZone
                });

                return transactions;
            }
        }

        //This method recieves a list of transactions from the database,
        //selecting only those columns that were specified in the user's request.
        public async Task<IEnumerable<dynamic>> GetSelectedColumnsTransactionsAsync(TransactionExportRequest request)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var selectedColumns = new List<string>();

                if (request.IncludeTransactionId) selectedColumns.Add("transaction_id AS TransactionId");
                if (request.IncludeName) selectedColumns.Add("name AS Name");
                if (request.IncludeEmail) selectedColumns.Add("email AS Email");
                if (request.IncludeAmount) selectedColumns.Add("amount AS Amount");
                if (request.IncludeTransactionDate) selectedColumns.Add("transaction_date AS TransactionDate");
                if (request.IncludeClientLocation) selectedColumns.Add("client_location AS ClientLocation");

                var sql = $"SELECT {string.Join(", ", selectedColumns)} FROM Transactions";

                var transactions = await connection.QueryAsync<dynamic>(sql);
                return transactions;
            }
        }
    }
}
