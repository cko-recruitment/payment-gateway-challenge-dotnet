using System.Text.Json.Serialization;

namespace PaymentGateway.Api.Models.Requests
{
    public record BankRequest
    {
        [JsonPropertyName("card_number")]
        public long CardNumber { get; set; }

        [JsonPropertyName("expiry_date")]
        public string ExpiryDate { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        [JsonPropertyName("amount")]
        public int Amount { get; set; }

        [JsonPropertyName("cvv")]
        public int Cvv { get; set; }
    }
}
