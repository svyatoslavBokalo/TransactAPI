using System.Transactions;
using TransactionAPI.Models;

namespace TransactionAPI.Services
{
    public interface ITransactionService
    {
        public Task<IEnumerable<TransactionModel>> GetTransactionsAsync();

        public Task AddOrUpdateTransactionAsync(TransactionModel transaction);
    }
}
