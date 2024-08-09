using CsvHelper;
using GeoTimeZone;
using Newtonsoft.Json.Linq;
using NodaTime;
using NodaTime.Text;
using System.Globalization;
using System.Reflection.Metadata;
using TransactionAPI.Models;

namespace TransactionAPI.BusinessLogic
{
    // яI took the logic of the parser and other methods to another class
    // due to the fact that the methods themselves in the controller/service will be very large
    // and besides other methods in the controller shouldn't be
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
            DateTime convertedTimeToUTC0 = ConvertToGeneralTimeZone(rows[4], timeZone);
            return new GeneralTimeModel()
                        {
                            TransactionId = rows[0],
                            TimeZone = timeZone,
                            GeneralTime = convertedTimeToUTC0
                        };
        }


        // this method for get time zone using API (http://api.timezonedb.com/).
        static public async Task<string> GetTimeZoneInfo(string coordinates)
        {
            var coords = coordinates.Split(',');
            double latitude = double.Parse(coords[0]);
            double longitude = double.Parse(coords[1]);

            // Using the TimeZoneDB API to get a time zone by coordinates
            // In GeneralConst, we have a static public parameter (ApiKey), which is the key to the API 
            string apiKey = GeneralConst.ApiKey;
            string url = $"http://api.timezonedb.com/v2.1/get-time-zone?key={apiKey}&format=json&by=position&lat={latitude}&lng={longitude}";

            var response = await client.GetStringAsync(url);
            var json = JObject.Parse(response);
            string timeZoneId = json["zoneName"].ToString();
            int gmtOffset = int.Parse(json["gmtOffset"].ToString()) / 3600;


            var dateTimeZone = DateTimeZoneProviders.Tzdb[timeZoneId];
            string result = $"{timeZoneId} UTC{(gmtOffset >= 0 ? "+" : "")}{gmtOffset}";

            return result;
        }

        // get time zone using TimeZoneLookup.GetTimeZone
        static public string GetTimeZoneFromCoordinates(string coordinates)
        {
            var coord = coordinates.Split(',');
            double latitude = double.Parse(coord[0]);
            double longitude = double.Parse(coord[1]);

            var timeZoneId = TimeZoneLookup.GetTimeZone(latitude, longitude).Result;

            return timeZoneId;
        }

        // this method convert to general time zone
        // example: we have UTC +9 -> UTC +0 
        static public DateTime ConvertToGeneralTimeZone(string dateTimeString, string timeZoneId)
        {
            // Delete everything that comes after the space in the name of the time zone
            if (timeZoneId.Contains(" "))
            {
                timeZoneId = timeZoneId.Split(' ')[0];
            }

            // Parsing date and time
            LocalDateTime localDateTime = LocalDateTime.FromDateTime(DateTime.Parse(dateTimeString));

            // Getting the time zone
            DateTimeZone timeZone = DateTimeZoneProviders.Tzdb[timeZoneId];

            // Convert local time to ZonedDateTime (date and time with time zone)
            ZonedDateTime zonedDateTime = localDateTime.InZoneLeniently(timeZone);

            // This code converts time from a specific time zone to UTC format and returns it as a DateTime object.
            Instant instant = zonedDateTime.ToInstant();
            DateTime utcDateTime = instant.ToDateTimeUtc();

            return utcDateTime;
        }
    }
}
