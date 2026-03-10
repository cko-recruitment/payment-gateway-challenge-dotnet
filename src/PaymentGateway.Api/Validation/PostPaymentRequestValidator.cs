using FluentValidation;

using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Validation;

public class PostPaymentRequestValidator : AbstractValidator<PostPaymentRequest>
{
    public PostPaymentRequestValidator()
    {
        RuleFor(x => x.CardNumber)
            .NotNull().WithMessage("Card number is required")
            .Length(14, 19).WithMessage("Card number must be between 14 and 19 characters long")
            .Matches(@"^[0-9]+$").WithMessage("Card number must only contain numeric characters");

        RuleFor(x => x.ExpiryMonth)
            .NotNull().WithMessage("Expiry month is required")
            .InclusiveBetween(1, 12).WithMessage("Expiry month must be between 1 and 12");

        RuleFor(x => x.ExpiryYear)
            .NotNull().WithMessage("Expiry year is required")
            .GreaterThanOrEqualTo(DateTime.UtcNow.Year)
            .WithMessage("Expiry year must be in the future");

        RuleFor(x => x).Must(IsDateInTheFuture)
            .WithMessage("Card expiry date must be in the future");

        RuleFor(x => x.Currency)
            .NotNull().WithMessage("Currency is required")
            .Length(3).WithMessage("Currency must be 3 characters long")
            .Must(x => x != null && AvailableCurrencies.Contains(x.ToUpper()))
            .WithMessage("Currency must be one of the following: USD, EUR, GBP");

        RuleFor(x => x.Amount)
            .NotNull().WithMessage("Amount is required")
            .GreaterThan(0).WithMessage("Amount must be greater than 0");

        RuleFor(x => x.CVV)
            .NotNull().WithMessage("CVV is required")
            .Length(3, 4).WithMessage("CVV must be 3 or 4 characters long")
            .Matches(@"^[0-9]+$").WithMessage("CVV must only contain numeric characters");
    }

    private static readonly HashSet<string> AvailableCurrencies = new HashSet<string> { "USD", "EUR", "GBP" };

    private static bool IsDateInTheFuture(PostPaymentRequest request)
    {
        var now = DateTime.UtcNow;
        return request.ExpiryYear > now.Year ||
               (request.ExpiryYear == now.Year && request.ExpiryMonth > now.Month);
    }
}