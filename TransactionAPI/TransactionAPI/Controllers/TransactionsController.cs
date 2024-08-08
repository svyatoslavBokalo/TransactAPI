using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Formats.Asn1;
using System.Globalization;
using TransactionAPI.BusinessLogic;
using TransactionAPI.Models;
using TransactionAPI.Services.Implementation;

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

        [HttpPost("importFile")]
        public async Task<IActionResult> ImportTransactions(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Please upload a valid CSV file.");
            }

            var transactions = new List<TransactionModel>();

            using (var stream = new StreamReader(file.OpenReadStream()))
            using (var csv = new CsvReader(stream, CultureInfo.InvariantCulture))
            {
                csv.Read();
                csv.ReadHeader();

                while (csv.Read())
                {
                    try
                    {
                        var transaction = BusinessClass.ParseFromCSV(csv);


                        transactions.Add(transaction);
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine($"Error reading row: {ex.Message}");
                    }
                }
            }

            foreach (var transaction in transactions)
            {
                await _transactionService.AddOrUpdateTransactionAsync(transaction);
            }

            return Ok();
        }
        private DateTime ParseDate(string dateStr)
        {
            DateTime date;
            if (!DateTime.TryParseExact(dateStr, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            {
                // Handle parsing error or assign a default value
                date = DateTime.MinValue; // or throw an exception if necessary
            }
            return date;
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
