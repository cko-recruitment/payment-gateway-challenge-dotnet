using System.Text.Json.Serialization;

namespace PaymentGateway.Api.Models.Requests
{
    internal class BankPropertyName()
    {
        public const string CardNumber = "card_number";

        public const string ExpiryDate = "expiry_date";

        public const string Currency = "currency";

        public const string Amount = "amount";

        public const string Cvv = "cvv";
    }

    /// <summary>
    /// Object for serializing bank request with correct property names
    /// </summary>
    public record BankRequest
    {
        [JsonPropertyName(BankPropertyName.CardNumber)]
        public long CardNumber { get; set; }

        [JsonPropertyName(BankPropertyName.ExpiryDate)]
        public string ExpiryDate { get; set; }

        [JsonPropertyName(BankPropertyName.Currency)]
        public string Currency { get; set; }

        [JsonPropertyName(BankPropertyName.Amount)]
        public int Amount { get; set; }

        [JsonPropertyName(BankPropertyName.Cvv)]
        public int Cvv { get; set; }
    }
}
