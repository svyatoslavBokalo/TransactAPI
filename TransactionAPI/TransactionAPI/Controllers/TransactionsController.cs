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


        // in this method I get all the data from the db
        [HttpGet("GetAllTransactions")]
        public async Task<IEnumerable<TransactionModel>> GetTransactions()
        {
            return await _transactionService.GetTransactionsAsync();
        }


        // in this method, I insert the data into the database, but through the API, so you need to register on this site(https://timezonedb.com/) and get ApiKay
        [HttpPost("add transaction using API (https:/timezonedb.com/ (but if you want to use this api it should register in this site" +
            "and get apiKey then change GeneralConst.ApiKey))")]
        public async Task<IActionResult> ImportTransaction([FromBody] TransactionModel transaction)
        {
            if (transaction == null)
            {
                return BadRequest("Invalid transaction data.");
            }

            // Getting the time zone
            var timeZone = await BusinessClass.GetTimeZoneInfo(transaction.ClientLocation);

            //Calculation of total time 
            var generalTimeUtc = BusinessClass.ConvertToGeneralTimeZone(transaction.TransactionDate.ToString("yyyy-MM-ddTHH:mm:ss"), timeZone);

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


        // in this method we import the csv file to db
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

        // in this method I did task 4 (get data in certain time zone)
        [HttpGet("transactions(certain time zone) -> user search transactions which are in the period of a certain time and certain time zone")]
        public async Task<IActionResult> GetTransactionsInUserTimeZone(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] string userCoordinates)
        {
            // Отримуємо транзакції з сервісу
            var transactions = await _transactionService.GetTransactionsInUserTimeZoneAsync(startDate, endDate, userCoordinates);

            return Ok(transactions);
        }


        // in this method I did task 5 (get all data in certain period)
        [HttpGet("transactions(all time zone) -> user search transactions which are in the period of a certain time in all time zone")]
        public async Task<IActionResult> GetTransactionsInPeriodTime(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] string userCoordinates)
        {
            var transactions = await _transactionService.GetTransactionsAllTimeZoneAndPeriodTimeAsync(startDate, endDate, userCoordinates);

            return Ok(transactions);
        }

        // in this method I did task 6 (get all data in january 2024)
        [HttpGet("transactions(in January 2024) -> user search transactions which occurred in January 2024 across all time zones")]
        public async Task<IActionResult> GetTransactionsInJanuary([FromQuery] string userCoordinates)
        {
            // Отримуємо транзакції з сервісу
            var transactions = await _transactionService.GetTransactionsAllTimeZoneAndPeriodTimeAsync(new DateTime(2024,01,01), new DateTime(2024,01,02), userCoordinates);

            return Ok(transactions);
        }

        // in this method we can export csv file with certain columns
        [HttpGet("export-transactions")]
        public async Task<IActionResult> ExportTransactionsToCsv([FromQuery] TransactionExportRequest request)
        {
            var transactions = await _transactionService.GetSelectedColumnsTransactionsAsync(request);

            var memoryStream = new MemoryStream();
            var streamWriter = new StreamWriter(memoryStream);
            var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);

            // Write column titles
            foreach (var header in ((IDictionary<string, object>)transactions.First()).Keys)
            {
                csvWriter.WriteField(header);
            }
            csvWriter.NextRecord();

            // Write data
            foreach (var transaction in transactions)
            {
                foreach (var field in ((IDictionary<string, object>)transaction).Values)
                {
                    csvWriter.WriteField(field);
                }
                csvWriter.NextRecord();
            }

            await csvWriter.FlushAsync();
            await streamWriter.FlushAsync();
            memoryStream.Position = 0;

            return File(memoryStream, "text/csv", "transactions.csv");
        }

    }
}
