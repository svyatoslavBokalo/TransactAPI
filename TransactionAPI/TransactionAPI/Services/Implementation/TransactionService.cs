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

        public async Task<IEnumerable<TransactionModel>> GetTransactionsAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = "SELECT transaction_id AS TransactionId, name, email, amount, transaction_date AS TransactionDate, client_location AS ClientLocation FROM Transactions";
                return await connection.QueryAsync<TransactionModel>(sql);
            }
        }

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

        public async Task<IEnumerable<TransactionModel>> GetTransactionsInUserTimeZoneAsync(DateTime startDate, DateTime endDate, string userCoordinates)
        {
            // Отримуємо часову зону користувача на основі його координат
            string userTimeZone = BusinessClass.GetTimeZoneFromCoordinates(userCoordinates);

            // Конвертуємо діапазон дат у UTC
            var startDateUtc = BusinessClass.ConvertToUtc(startDate.ToString(), userTimeZone);
            var endDateUtc = BusinessClass.ConvertToUtc(endDate.ToString(), userTimeZone);

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

        public async Task<IEnumerable<TransactionModel>> GetTransactionsAllTimeZoneAndPeriodTimeAsync(DateTime startDate, DateTime endDate, string userCoordinates)
        {
            // Отримуємо часову зону користувача на основі його координат
            string userTimeZone = BusinessClass.GetTimeZoneFromCoordinates(userCoordinates);

            // Конвертуємо діапазон дат у UTC
            var startDateUtc = BusinessClass.ConvertToUtc(startDate.ToString(), userTimeZone);
            var endDateUtc = BusinessClass.ConvertToUtc(endDate.ToString(), userTimeZone);

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
    }
}
