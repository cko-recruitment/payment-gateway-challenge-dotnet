using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Common.Extensions;

public static class PostPaymentRequestExtensions
{
    public static BankSimulatorRequest ToBankSimulatorRequest(this PostPaymentRequest request) =>
        new()
        {
            CardNumber = request.CardNumber,
            ExpiryDate = $"{request.ExpiryMonth:D2}/{request.ExpiryYear}",
            Currency = request.Currency,
            Amount = request.Amount,
            Cvv = request.CVV
        };

    public static int GetCardNumberLastFour(this PostPaymentRequest request) =>
        int.TryParse(request.CardNumber?[^Math.Min(request.CardNumber.Length, 4)..], out var last4)
            ? last4
            : 0;
}