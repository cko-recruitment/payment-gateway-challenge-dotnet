using FluentValidation;
using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Validators;

public class PostPaymentRequestValidator : AbstractValidator<PostPaymentRequest>
{
    private static readonly HashSet<string> SupportedCurrencies = new(StringComparer.OrdinalIgnoreCase)
    {
        "GBP",
        "USD",
        "EUR"
    };

    public PostPaymentRequestValidator()
    {
        RuleFor(x => x.CardNumber)
            .NotEmpty().WithMessage("Card number is required.")
            .Length(14, 19).WithMessage("Card number must be between 14 and 19 characters long.")
            .Matches("^[0-9]+$").WithMessage("Card number must only contain numeric characters.");

        RuleFor(x => x.ExpiryMonth)
            .InclusiveBetween(1, 12).WithMessage("Expiry month must be between 1 and 12.");

        RuleFor(x => x.ExpiryYear)
            .GreaterThan(0).WithMessage("Expiry year is required.");

        RuleFor(x => x)
            .Must(HaveFutureExpiryDate)
            .WithMessage("Expiry month and year must be in the future.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Length(3).WithMessage("Currency must be 3 characters.")
            .Must(currency => SupportedCurrencies.Contains(currency))
            .WithMessage("Currency must be one of GBP, USD, or EUR.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be an integer greater than 0.");

        RuleFor(x => x.Cvv)
            .NotEmpty().WithMessage("CVV is required.")
            .Length(3, 4).WithMessage("CVV must be between 3 and 4 characters long.")
            .Matches("^[0-9]+$").WithMessage("CVV must only contain numeric characters.");
    }

    private static bool HaveFutureExpiryDate(PostPaymentRequest request)
    {
        if (request.ExpiryMonth is < 1 or > 12 || request.ExpiryYear <= 0)
        {
            return false;
        }

        var expiryDate = new DateTime(request.ExpiryYear, request.ExpiryMonth, 1).AddMonths(1);
        return expiryDate > DateTime.UtcNow.Date;
    }
}
