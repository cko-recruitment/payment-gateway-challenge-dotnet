using System.Text.Json.Serialization;

namespace PaymentGateway.Api.Models.BankSimulator;

public class BankPaymentRequest
{
    [JsonPropertyName("card_number")]
    public string CardNumber { get; set; } = string.Empty;

    [JsonPropertyName("expiry_date")]
    public string ExpiryDate { get; set; } = string.Empty;

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public int Amount { get; set; }

    [JsonPropertyName("cvv")]
    public string Cvv { get; set; } = string.Empty;
}
