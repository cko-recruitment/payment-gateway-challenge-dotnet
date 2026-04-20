using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Extensions;

public static class PaymentExtensions
{
    public static BankPaymentRequest ToBankPaymentRequest(this PostPaymentRequest request)
    {
        return new BankPaymentRequest
        {
            CardNumber = request.CardNumber.ToString(),
            ExpiryDate = $"{request.ExpiryMonth:D2}/{request.ExpiryYear}",
            Currency = request.Currency,
            Amount = request.Amount,
            Cvv = request.Cvv.ToString()
        };
    }

    public static PaymentResponse ToPaymentResponse(this Payment payment)
    {
        return new PaymentResponse
        {
            Id = payment.Id,
            CardNumberLastFour = payment.CardNumberLastFour,
            ExpiryMonth = payment.ExpiryMonth,
            ExpiryYear = payment.ExpiryYear,
            Currency = payment.Currency,
            Amount = payment.Amount,
            Status = payment.Status
        };
    }
}