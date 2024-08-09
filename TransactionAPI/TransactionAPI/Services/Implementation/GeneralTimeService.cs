using Dapper;
using Microsoft.Data.SqlClient;
using TransactionAPI.Models;
using TransactionAPI.Services.Interfaces;

namespace TransactionAPI.Services.Implementation
{
    public class GeneralTimeService : IGeneralTimeService
    {

        private readonly string _connectionString;

        public GeneralTimeService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // in this method we add or update our data which we receive from csv file and calculate for GeneralTimes table
        public async Task AddOrUpdateGeneralTimeAsync(GeneralTimeModel generalTime)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"
                IF EXISTS (SELECT 1 FROM GeneralTimes WHERE transaction_id = @TransactionId)
                BEGIN
                    UPDATE GeneralTimes
                    SET timezone = @TimeZone,
                        general_time = @GeneralTime
                    WHERE transaction_id = @TransactionId
                END
                ELSE
                BEGIN
                    INSERT INTO GeneralTimes (transaction_id, timezone, general_time)
                    VALUES (@TransactionId, @TimeZone, @GeneralTime)
                END";

                var parameters = new
                {
                    TransactionId = generalTime.TransactionId,
                    TimeZone = generalTime.TimeZone,
                    GeneralTime = generalTime.GeneralTime
                };

                await connection.ExecuteAsync(sql, parameters);
            }
        }
    }
}
