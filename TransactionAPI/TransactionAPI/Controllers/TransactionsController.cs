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
        private readonly GeneralTimeService _generalTimeService;

        public TransactionsController(TransactionService transactionService, GeneralTimeService generalTimeService)
        {
            this._transactionService = transactionService;
            this._generalTimeService = generalTimeService;
        }

        [HttpGet("GetAllTransactions")]
        public async Task<IEnumerable<TransactionModel>> GetTransactions()
        {
            return await _transactionService.GetTransactionsAsync();
        }

        //[HttpPost("addOneTransaction")]
        //public async Task<IActionResult> ImportTransaction([FromBody] TransactionModel transaction)
        //{
        //    await _transactionService.AddOrUpdateTransactionAsync(transaction);
        //    GeneralTimeModel generalTime = new GeneralTimeModel();
        //    generalTime.TransactionId = transaction.TransactionId;
        //    generalTime.TimeZone = await BusinessClass.GetTimeZoneInfo(transaction.ClientLocation);
        //    generalTime.GeneralTime = BusinessClass.ConvertToUtc(transaction.TransactionDate.ToString(), generalTime.TimeZone);
        //    await _generalTimeService.AddOrUpdateGeneralTimeAsync(generalTime);
        //    return Ok();
        //}

        [HttpPost("add transaction using API (https://timezonedb.com/ (but if you want to use this api it should to register in this site" +
            "and get apiKey then change GeneralConstClass.ApiKey))")]
        public async Task<IActionResult> ImportTransaction([FromBody] TransactionModel transaction)
        {
            if (transaction == null)
            {
                return BadRequest("Invalid transaction data.");
            }

            // Отримання часового поясу
            var timeZone = await BusinessClass.GetTimeZoneInfo(transaction.ClientLocation);

            // Розрахунок загального часу (UTC)
            var generalTimeUtc = BusinessClass.ConvertToUtc(transaction.TransactionDate.ToString("yyyy-MM-ddTHH:mm:ss"), timeZone);

            // Створення GeneralTimeModel
            var generalTime = new GeneralTimeModel
            {
                TransactionId = transaction.TransactionId,
                TimeZone = timeZone,
                GeneralTime = generalTimeUtc
            };

            await _transactionService.AddOrUpdateTransactionAsync(transaction);
            await _generalTimeService.AddOrUpdateGeneralTimeAsync(generalTime);

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
            var generalTimes = new List<GeneralTimeModel>();

            using (var stream = new StreamReader(file.OpenReadStream()))
            using (var csv = new CsvReader(stream, CultureInfo.InvariantCulture))
            {
                csv.Read();
                csv.ReadHeader();

                while (csv.Read())
                {
                    try
                    {
                        var transaction = BusinessClass.ParseTransaction(csv);
                        var generalTime = await BusinessClass.ParseGeneralTime(csv);


                        transactions.Add(transaction);
                        generalTimes.Add(generalTime);
                        
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine($"Error reading row: {ex.Message}");
                    }
                }
            }

            for(int i = 0; i < transactions.Count; i++)
            {
                await _transactionService.AddOrUpdateTransactionAsync(transactions[i]);
                await _generalTimeService.AddOrUpdateGeneralTimeAsync(generalTimes[i]);
            }

            return Ok();
        }
    }
}
