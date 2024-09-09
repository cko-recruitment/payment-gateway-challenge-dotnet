using System.ComponentModel.DataAnnotations;

using PaymentGateway.Api.Validation;

namespace PaymentGateway.Api.Models.Requests;

public class PostPaymentRequest
{
    [Required]
    [Length(14, 19, ErrorMessage = "Card Number is not between 14 and 19 characters in length")]
    public string CardNumber { get; set; }

    [Required]
    [Range(1, 12, ErrorMessage = "Expiry Month number is not between 1 and 12")]
    public int ExpiryMonth { get; set; }

    [Required]
    [ExpiryYearValidation]
    public int ExpiryYear { get; set; }

    [Required]
    [CurrencyValidation]
    public string Currency { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Amount supplied is not valid, ensure it's positive and greater or equal to 1")]
    public int Amount { get; set; }

    [Required]
    [Length(3, 4, ErrorMessage = "CVV is not between 3 or 4 characters in length")]
    public string Cvv { get; set; }
}