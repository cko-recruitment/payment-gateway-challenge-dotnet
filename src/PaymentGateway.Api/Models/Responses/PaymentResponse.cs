using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Models.Responses;

public class PaymentResponse
{
    public Guid Id { get; init; }
    public PaymentStatus Status { get; init; }
    public string CardNumberLastFour { get; init; } = string.Empty;
    public int ExpiryMonth { get; init; }
    public int ExpiryYear { get; init; }
    public string Currency { get; init; } = string.Empty;
    public int Amount { get; init; }
}

public static class PaymentExtensions
{
    extension(Payment payment)
    {
        public PaymentResponse ToResponse() =>
            new()
            {
                Id = payment.Id,
                Status = payment.BankAuthorization.Status,
                CardNumberLastFour = payment.CardNumber[^4..],
                ExpiryMonth = payment.ExpiryMonth,
                ExpiryYear = payment.ExpiryYear,
                Currency = payment.Currency,
                Amount = payment.Amount
            };
    }
}
    

