using Dapper;
using Microsoft.Data.SqlClient;
using System.Transactions;
using TransactionAPI.Models;

namespace TransactionAPI.Services.Implementation
{
    public class TransactionService
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
                    UPDATE GenerelTimes
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
                    INSERT INTO GenerelTimes (transaction_id, timezone, general_time)
                    VALUES (@TransactionId, @TimeZone, @GeneralTime)
                END";
                await connection.ExecuteAsync(sql, transaction);
            }
        }
    }
}
