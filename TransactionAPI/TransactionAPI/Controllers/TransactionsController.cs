using Microsoft.AspNetCore.Mvc;
using TransactionAPI.Models;
using TransactionAPI.Services;

namespace TransactionAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly TransactionService _transactionService;

        public TransactionsController(TransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpGet("GetAllTransactions")]
        public async Task<IEnumerable<TransactionModel>> GetTransactions()
        {
            return await _transactionService.GetTransactionsAsync();
        }

        [HttpPost("addOneTransaction")]
        public async Task<IActionResult> ImportTransaction([FromBody] TransactionModel transaction)
        {
            await _transactionService.AddOrUpdateTransactionAsync(transaction);
            return Ok();
        }

        //[HttpPost]
        //public async Task<IActionResult> ImportTransactions([FromBody] List<TransactionModel> transactions)
        //{
        //    foreach (var transaction in transactions)
        //    {
        //        await _transactionService.AddOrUpdateTransactionAsync(transaction);
        //    }
        //    return Ok();
        //}
    }
}
