using System.Transactions;
using TransactionAPI.Models;

namespace TransactionAPI.Services.Interfaces
{
    public interface ITransactionService
    {
        public Task<IEnumerable<TransactionModel>> GetTransactionsAsync();

        public Task AddOrUpdateTransactionAsync(TransactionModel transaction);
        public Task<IEnumerable<TransactionModel>> GetTransactionsInUserTimeZoneAsync(DateTime startDate, DateTime endDate, string userCoordinates);
    }
}
