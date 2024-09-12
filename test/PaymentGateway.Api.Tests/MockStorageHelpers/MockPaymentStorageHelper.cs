using System.Text.Json;

using PaymentGateway.Api.Constants.Enums;
using PaymentGateway.Api.Tests.MockStorageHelpers.Models;

namespace PaymentGateway.Api.Tests.MockStorageHelpers
{
    public class MockPaymentStorageHelper
    {
        public static string GenerateRandomPaymentAsString(Guid id)
        {
            var random = new Random();
            var payment = new Payment()
            {
                Id = id,
                Amount = (int)GenerateRandomNumber(1, 5),
                CardNumber = GenerateRandomNumber(14, 19).ToString(),
                Currency = GetRandomCurrency(random.Next(0, 2)),
                ExpiryMonth = random.Next(1, 13),
                ExpiryYear = random.Next(DateTime.Now.Year, DateTime.Now.Year + 10),
                Status = random.Next(0, 1) == 0 ? PaymentStatus.Authorized : PaymentStatus.Declined,
            };

            return JsonSerializer.Serialize(payment);
        }

        private static long GenerateRandomNumber(int minDigits, int maxDigits)
        {
            var random = new Random();
            // Choose a random number of digits between minDigits and maxDigits
            int numberOfDigits = random.Next(minDigits, maxDigits + 1);
            // Calculate the minimum and maximum values for the chosen number of digits
            long minValue = (long)Math.Pow(10, numberOfDigits - 1);
            long maxValue = (long)Math.Pow(10, numberOfDigits) - 1;
            // Generate random number between minValue and maxValue
            long randomNumber = (long)(random.NextDouble() * (maxValue - minValue) + minValue);

            return randomNumber;
        }

        private static string GetRandomCurrency(int randomNumber)
        {
            // Choose a currency based on the random number
            return randomNumber switch
            {
                0 => Currencies.GBP.ToString(),
                1 => Currencies.EUR.ToString(),
                2 => Currencies.USD.ToString(),
            };
        }
    }
}
