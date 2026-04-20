using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Api.Models.Requests;

/// <summary>
/// Payment request model containing card and transaction details.
/// </summary>
public class PostPaymentRequest
{
    /// <summary>
    /// The full card number (14-19 numeric digits).
    /// </summary>
    public string CardNumber { get; set; }

    /// <summary>
    /// Card expiry month (1-12).
    /// </summary>
    public int ExpiryMonth { get; set; }

    /// <summary>
    /// Card expiry year (YYYY format).
    /// </summary>
    public int ExpiryYear { get; set; }

    /// <summary>
    /// ISO 4217 currency code (e.g., GBP, USD, EUR).
    /// </summary>
    public string Currency { get; set; }

    /// <summary>
    /// Transaction amount in the smallest currency unit (e.g., pence for GBP).
    /// </summary>
    public int Amount { get; set; }

    /// <summary>
    /// Card verification value (3-4 numeric digits).
    /// </summary>
    public string Cvv { get; set; }
}