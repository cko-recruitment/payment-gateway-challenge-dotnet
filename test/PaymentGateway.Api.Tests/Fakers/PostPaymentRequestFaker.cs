using Bogus;
using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Tests.Fakers;

public class PostPaymentRequestFaker : Faker<PostPaymentRequest>
{
    public PostPaymentRequestFaker()
    {
        var currentYear = DateTime.UtcNow.Year;

        RuleFor(p => p.CardNumber, f => f.Random.String2(16, "0123456789"))
            .RuleFor(p => p.ExpiryMonth, f => f.Random.Int(1, 12))
            .RuleFor(p => p.ExpiryYear, currentYear + 2)  // Always future year by default
            .RuleFor(p => p.Currency, f => f.PickRandom(new[] { "GBP", "USD", "EUR" }))
            .RuleFor(p => p.Amount, f => f.Random.Int(100, 100000))  
            .RuleFor(p => p.Cvv, f => f.Random.String2(3, "0123456789"));
    }

    public PostPaymentRequestFaker WithCardNumber(string cardNumber)
    {
        RuleFor(p => p.CardNumber, cardNumber);
        return this;
    }

    public PostPaymentRequestFaker WithMonth(int month)
    {
        RuleFor(p => p.ExpiryMonth, month);
        return this;
    }

    public PostPaymentRequestFaker WithYear(int year)
    {
        RuleFor(p => p.ExpiryYear, year);
        return this;
    }
    public PostPaymentRequestFaker WithCurrency(string currency)
    {
        RuleFor(p => p.Currency, currency);
        return this;
    }
    public PostPaymentRequestFaker WithCvv(string cvv)
    {
        RuleFor(p => p.Cvv, cvv);
        return this;
    }
}