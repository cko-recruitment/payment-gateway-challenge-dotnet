using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Api.Models.Requests;

/// <summary>
/// The payment request object
/// </summary>
public class PostPaymentRequest
{
    // Mark as required so model binding on the controller prevents these from being null
    [Required]
    public long? CardNumber { get; set; }

    [Required]
    public int? ExpiryMonth { get; set; }

    [Required]
    public int? ExpiryYear { get; set; }

    [Required]
    public string? Currency { get; set; }

    [Required]
    public int? Amount { get; set; }

    [Required]
    public int? Cvv { get; set; }
}