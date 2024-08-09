using CsvHelper;
using GeoTimeZone;
using Newtonsoft.Json.Linq;
using NodaTime;
using System.Globalization;
using System.Reflection.Metadata;
using TransactionAPI.Models;

namespace TransactionAPI.BusinessLogic
{
    // я витяг логіку парсера в інший клас у зв'язку з тим, що сам метод в контролері буде дуже громадним
    // і окрім того ніякі інші методи в кнотролері не повинні бути окрім тих які виконують логіку сервісів
    static public class BusinessClass
    {
        private static readonly HttpClient client = new HttpClient();

        // this method so that the code is not repeated, because it's called both in "ParseTransaction" and "ParseGeneralTime" 
        static public List<string> GetRowFromCSV(CsvReader csv)
        {
            return new List<string> { csv.GetField("transaction_id") ,
                                        csv.GetField("name"),
                                        csv.GetField("email"),
                                        csv.GetField("amount").Replace("$", "").Trim(),
                                        csv.GetField("transaction_date"),
                                        csv.GetField("client_location"),
                                    };
        }

        // this method is parser transaction from csv file
        static public TransactionModel ParseTransaction(CsvReader csv)
        {
            var dateFormats = new[] { "dd/MM/yyyy HH:mm", "yyyy-MM-dd HH:mm:ss" };

            List<string> rows = GetRowFromCSV(csv);
            var transactionDateStr = rows[4];
            DateTime transactionDate = new DateTime();
            if (string.IsNullOrWhiteSpace(transactionDateStr) ||
                !DateTime.TryParseExact(transactionDateStr, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out transactionDate))
            {
                System.Console.WriteLine($"Invalid or empty date format for transaction_id: {rows[0]}, date: {transactionDateStr}");
            }

            var transaction = new TransactionModel
            {
                TransactionId = rows[0],
                Name = rows[1],
                Email = rows[2],
                Amount = decimal.Parse(rows[3]),
                TransactionDate = transactionDate,
                ClientLocation = rows[5]
            };

            return transaction;
        }

        // this method is parser GeneralTimeModel from csv file
        static public async Task<GeneralTimeModel> ParseGeneralTime(CsvReader csv)
        {
            List<string> rows = GetRowFromCSV(csv);
            string timeZone = GetTimeZoneFromCoordinates(rows[5]);
            DateTime convertedTimeToUTC0 = ConvertToUtc(rows[4], timeZone);
            return new GeneralTimeModel()
                        {
                            TransactionId = rows[0],
                            TimeZone = timeZone,
                            GeneralTime = convertedTimeToUTC0
                        };
        }

        static public async Task<string> GetTimeZoneInfo(string coordinates)
        {
            var coords = coordinates.Split(',');
            double latitude = double.Parse(coords[0]);
            double longitude = double.Parse(coords[1]);

            // Використання TimeZoneDB API для отримання тайм-зони за координатами
            string apiKey = "FG8RG8JQRG4Y";
            string url = $"http://api.timezonedb.com/v2.1/get-time-zone?key={apiKey}&format=json&by=position&lat={latitude}&lng={longitude}";

            var response = await client.GetStringAsync(url);
            var json = JObject.Parse(response);
            string timeZoneId = json["zoneName"].ToString();
            int gmtOffset = int.Parse(json["gmtOffset"].ToString()) / 3600;

            // Форматування результату
            var dateTimeZone = DateTimeZoneProviders.Tzdb[timeZoneId];
            string result = $"{timeZoneId} UTC{(gmtOffset >= 0 ? "+" : "")}{gmtOffset}";

            return result;
        }

        static public string GetTimeZoneFromCoordinates(string coordinates)
        {
            var coord = coordinates.Split(',');
            double latitude = double.Parse(coord[0]);
            double longitude = double.Parse(coord[1]);

            // Використання GeoTimeZone для отримання ID тайм-зони IANA
            var timeZoneId = TimeZoneLookup.GetTimeZone(latitude, longitude).Result;

            return timeZoneId;
        }

        static public DateTime ConvertToUtc(string dateTimeString, string timeZoneId)
        {
            // Видаляємо все, що йде після пробілу в імені тайм-зони (якщо є суфікс на кшталт UTC-4)
            if (timeZoneId.Contains(" "))
            {
                timeZoneId = timeZoneId.Split(' ')[0];
            }

            // Парсинг дати та часу
            LocalDateTime localDateTime = LocalDateTime.FromDateTime(DateTime.Parse(dateTimeString));

            // Отримання часового поясу
            DateTimeZone timeZone = DateTimeZoneProviders.Tzdb[timeZoneId];

            // Перетворення локального часу в ZonedDateTime (дата і час з часовою зоною)
            ZonedDateTime zonedDateTime = localDateTime.InZoneLeniently(timeZone);

            // Перетворення в UTC
            Instant instant = zonedDateTime.ToInstant();
            DateTime utcDateTime = instant.ToDateTimeUtc();

            return utcDateTime;
        }
    }
}
