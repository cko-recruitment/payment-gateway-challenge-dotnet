using PaymentGateway.Api.Models.Domain;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Common.Mapping;

public static class PaymentMappingExtensions
{
    public static PaymentResponse ToPaymentResponse(this Payment payment) =>
        new()
        {
            Id = payment.Id,
            Status = payment.Status,
            CardNumberLastFour = payment.CardNumberLastFour,
            ExpiryMonth = payment.ExpiryMonth,
            ExpiryYear = payment.ExpiryYear,
            Currency = payment.Currency,
            Amount = payment.Amount
        };
}