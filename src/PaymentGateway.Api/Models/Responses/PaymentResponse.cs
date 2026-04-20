using System.Text.Json.Serialization;

using PaymentGateway.Api.Enums;

namespace PaymentGateway.Api.Models.Responses;

/// <summary>
/// Payment response model containing payment details and status.
/// </summary>
    public class PaymentResponse
{
    /// <summary>
    /// Unique identifier for the payment.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Payment status: Authorized, Declined, or Rejected.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PaymentStatus Status { get; set; }

    /// <summary>
    /// Last four digits of the card number.
    /// </summary>
    public int CardNumberLastFour { get; set; }

    /// <summary>
    /// Card expiry month (1-12).
    /// </summary>
    public int ExpiryMonth { get; set; }

    /// <summary>
    /// Card expiry year (YYYY format).
    /// </summary>
    public int ExpiryYear { get; set; }

    /// <summary>
    /// ISO 4217 currency code used for the transaction.
    /// </summary>
    public string Currency { get; set; }

    /// <summary>
    /// Transaction amount in the smallest currency unit.
    /// </summary>
    public int Amount { get; set; }
}