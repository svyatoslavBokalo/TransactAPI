using System.Transactions;
using TransactionAPI.Models;

namespace TransactionAPI.Services.Interfaces
{
    public interface ITransactionService
    {
        public Task<IEnumerable<TransactionModel>> GetTransactionsAsync();

        public Task AddOrUpdateTransactionAsync(TransactionModel transaction);
    }
}
