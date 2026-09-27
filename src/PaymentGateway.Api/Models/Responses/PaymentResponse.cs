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

public static class PaymentResponseExtensions
{
    extension(PaymentModel paymentModel)
    {
        public PaymentResponse ToResponse() =>
            new()
            {
                Id = paymentModel.Id,
                Status = paymentModel.BankAuthorization.Status,
                CardNumberLastFour = paymentModel.CardNumber[^4..],
                ExpiryMonth = paymentModel.ExpiryMonth,
                ExpiryYear = paymentModel.ExpiryYear,
                Currency = paymentModel.Currency,
                Amount = paymentModel.Amount
            };
    }
}
    

