using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Models.Domain;
using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Tests.Helpers;

public class PaymentTestDataBuilder
{
    private static readonly Random Random = new();
    private static readonly string[] SupportedCurrencies = ["GBP", "USD", "EUR"];

    private const string AuthorizedCardNumber = "1234567812341111";
    private const string DeclinedCardNumber = "1234567812342222";
    private const string UnavailableCardNumber = "1234567812340000";

    public static PostPaymentRequest BuildAuthorizedRequest() =>
        BuildRequest(AuthorizedCardNumber);

    public static PostPaymentRequest BuildDeclinedRequest() =>
        BuildRequest(DeclinedCardNumber);

    public static PostPaymentRequest BuildUnavailableRequest() =>
        BuildRequest(UnavailableCardNumber);

    public static PostPaymentRequest BuildRequest(string cardNumber) => new()
    {
        CardNumber = cardNumber,
        ExpiryMonth = Random.Next(1, 13),
        ExpiryYear = Random.Next(DateTime.Now.Year + 1, DateTime.Now.Year + 10),
        Currency = RandomCurrency(),
        Amount = Random.Next(1, 10000),
        CVV = Random.Next(100, 9999).ToString()
    };

    public static Payment BuildPayment(
        PaymentStatus status = PaymentStatus.Authorized,
        string? idempotencyKey = null) => new()
    {
        Id = Guid.NewGuid(),
        CardNumberLastFour = Random.Next(1000, 9999),
        ExpiryMonth = Random.Next(1, 13),
        ExpiryYear = Random.Next(DateTime.Now.Year + 1, DateTime.Now.Year + 10),
        Currency = RandomCurrency(),
        Amount = Random.Next(1, 10000),
        Status = status,
        AuthorizationCode = status == PaymentStatus.Authorized ? Guid.NewGuid().ToString() : null,
        IdempotencyKey = idempotencyKey
    };
    
    public static PostPaymentRequest BuildInvalidRequest() => new()
    {
        CardNumber = "1234a",  // non-numeric
        ExpiryMonth = 13,      // invalid month
        ExpiryYear = 2020,     // in the past
        Currency = "AUD",      // unsupported
        Amount = -1,           // negative
        CVV = "1"              // too short
    };

    private static string RandomCurrency() =>
        SupportedCurrencies[Random.Next(SupportedCurrencies.Length)];
}