using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Services;

public record ValidationError(string Field, string[] Messages);

public static class PaymentValidator
{
    private static readonly HashSet<string> SupportedCurrencies = ["GBP", "USD", "EUR"]; // add all supported currencies here

    // I'd use a discriminated union or a Result type for the return type in production for better type safety and readability.
    // C# haven't yet introduced the union in the latest released version, so I stick to the nullable type for now.
    public static ValidationError? Validate(CreatePaymentRequest request, DateTime today) =>
        request switch
        {
            { CardNumber.Length: < 14 or > 19 } => new(nameof(request.CardNumber),
                ["Card number must contain 14 to 19 digits."]),

            { CardNumber: var cardNumber } when !IsDigits(cardNumber) => new(nameof(request.CardNumber),
                ["Card number must contain only digits."]),

            { ExpiryMonth: null } => new(nameof(request.ExpiryMonth), ["Expiry month is required."]),

            { ExpiryMonth: < 0 or > 12 } => new(nameof(request.ExpiryMonth),
                ["Expiry month must be between 1 and 12."]),

            { ExpiryYear: null } => new(nameof(request.ExpiryYear), ["Expiry year is required."]),

            { ExpiryYear: var year, ExpiryMonth: var month } when !IsInFuture(year.Value, month.Value, today) => new
                (nameof(request.ExpiryYear), ["Expiry month and year must not be in the past."]),

            { Currency: null } => new(nameof(request.Currency), ["Currency is required."]),

            { Currency: var currency } when !SupportedCurrencies.Contains(currency.ToUpper()) => new(
                nameof(request.Currency), ["Currency is not supported."]),

            { Amount: < 0 } => new(nameof(request.Amount),
                ["Amount is required and must be a positive integer in minor units."]),

            { Cvv: null } => new(nameof(request.Cvv), ["CVV is required."]),

            { Cvv.Length: < 3 or > 4 } => new(nameof(request.Cvv), ["CVV must contain 3 or 4 digits."]),

            { Cvv: var cvv } when !IsDigits(cvv) => new(nameof(request.Cvv), ["CVV must contain only digits."]),

            _ => null
        };

    private static bool IsDigits(string value) =>  value.All(character => character is >= '0' and <= '9');
    private static bool IsInFuture(int year, int month, DateTime today) => (year * 12 + month) > (today.Year * 12 + today.Month);
}