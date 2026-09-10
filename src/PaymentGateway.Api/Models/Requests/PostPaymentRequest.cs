using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Api.Models.Requests;

public class PostPaymentRequest : IValidatableObject
{
    // Only currencies the bank simulator / this gateway is validated against.
    public static readonly string[] AllowedCurrencies = { "USD", "GBP", "EUR" };

    [Required]
    [RegularExpression(@"^\d{14,19}$", ErrorMessage = "Card number must be between 14 and 19 numeric characters")]
    public string CardNumber { get; set; } = string.Empty;

    [Required]
    [Range(1, 12, ErrorMessage = "Expiry month must be between 1 and 12")]
    public int ExpiryMonth { get; set; }

    [Required]
    [Range(1, 9999, ErrorMessage = "Expiry year is required")]
    public int ExpiryYear { get; set; }

    [Required]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency must be 3 characters")]
    public string Currency { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Amount must be a positive integer")]
    public int Amount { get; set; }

    [Required]
    [RegularExpression(@"^\d{3,4}$", ErrorMessage = "CVV must be between 3 and 4 numeric characters")]
    public string Cvv { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrWhiteSpace(Currency) && !AllowedCurrencies.Contains(Currency, StringComparer.OrdinalIgnoreCase))
        {
            yield return new ValidationResult(
                $"Currency must be one of the following: {string.Join(", ", AllowedCurrencies)}",
                new[] { nameof(Currency) });
        }

        if (ExpiryMonth is >= 1 and <= 12 && ExpiryYear is >= 1 and <= 9999)
        {
            var expiryDate = new DateOnly(ExpiryYear, ExpiryMonth, 1).AddMonths(1).AddDays(-1);
            if (expiryDate < DateOnly.FromDateTime(DateTime.UtcNow))
            {
                yield return new ValidationResult(
                    "Expiry month and year must be in the future",
                    new[] { nameof(ExpiryMonth), nameof(ExpiryYear) });
            }
        }
    }
}