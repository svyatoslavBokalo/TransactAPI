using CsvHelper;
using System.Globalization;
using TransactionAPI.Models;

namespace TransactionAPI.BusinessLogic
{
    // я витяг логіку парсера в інший клас у зв'язку з тим, що сам метод в контролері буде дуже громадним
    // і окрім того ніякі інші методи в кнотролері не повинні бути окрім тих які виконують логіку сервісів
    static public class ExcelWorking
    {

        static public TransactionModel ParseFromCSV(CsvReader csv)
        {
            var dateFormats = new[] { "dd/MM/yyyy HH:mm", "yyyy-MM-dd HH:mm:ss" };

            var transaction = new TransactionModel
            {
                TransactionId = csv.GetField("transaction_id"),
                Name = csv.GetField("name"),
                Email = csv.GetField("email"),
                Amount = decimal.Parse(csv.GetField("amount").Replace("$", "").Trim()),
                ClientLocation = csv.GetField("client_location")
            };

            var transactionDateStr = csv.GetField("transaction_date");
            DateTime transactionDate = new DateTime();
            if (string.IsNullOrWhiteSpace(transactionDateStr) ||
                !DateTime.TryParseExact(transactionDateStr, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out transactionDate))
            {
                System.Console.WriteLine($"Invalid or empty date format for transaction_id: {transaction.TransactionId}, date: {transactionDateStr}");
            }

            transaction.TransactionDate = transactionDate;

            return transaction;
        }
    }
}
