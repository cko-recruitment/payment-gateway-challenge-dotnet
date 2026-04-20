using FluentValidation;
using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Validators;

public class PostPaymentRequestValidator : AbstractValidator<PostPaymentRequest>
{
    public PostPaymentRequestValidator()
    {
        RuleFor(x => x.CardNumber)
            .NotEmpty()
            .WithMessage("Card number is required.")
            .Length(14, 19)
            .WithMessage("Card number must be between 14-19 characters long.")
            .Matches("^[0-9]+$")
            .WithMessage("Card number must contain only numeric characters.");

        RuleFor(x => x.ExpiryMonth)
            .NotEmpty()
            .WithMessage("Expiry month is required.")
            .InclusiveBetween(1, 12)
            .WithMessage("Expiry month must be between 1 and 12.");

        RuleFor(x => x.ExpiryYear)
            .NotEmpty()
            .WithMessage("Expiry year is required.");

        RuleFor(x => new { x.ExpiryMonth, x.ExpiryYear })
            .Must(x => x.ExpiryYear > DateTime.UtcNow.Year 
                || (x.ExpiryYear == DateTime.UtcNow.Year && x.ExpiryMonth > DateTime.UtcNow.Month))
            .WithName("ExpiryDate")
            .WithMessage("The expiry date must be in the future.");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Currency is required.")
            .Length(3, 3)
            .WithMessage("Currency must be exactly 3 characters.")
            .Must(currency => new[] { "GBP", "USD", "EUR" }.Contains(currency.ToUpperInvariant()))
            .WithMessage("Currency must be one of: GBP, USD, EUR.");

        RuleFor(x => x.Amount)
            .NotEmpty()
            .WithMessage("Amount is required.")
            .GreaterThan(0)
            .WithMessage("Amount must be greater than 0.");

        RuleFor(x => x.Cvv)
            .NotEmpty()
            .WithMessage("CVV is required.")
            .Length(3, 4)
            .WithMessage("CVV must be between 3-4 characters long.")
            .Matches("^[0-9]+$")
            .WithMessage("CVV must contain only numeric characters.");
    }
}
