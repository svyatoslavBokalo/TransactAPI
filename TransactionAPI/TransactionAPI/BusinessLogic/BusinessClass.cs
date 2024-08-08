﻿using CsvHelper;
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

        static async Task<string> GetTimeZoneInfo(string dateTimeString, string coordinates)
        {
            DateTime dateTime = DateTime.Parse(dateTimeString);
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

        static DateTime ConvertToUtc(string dateTimeString, string timeZoneId)
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
