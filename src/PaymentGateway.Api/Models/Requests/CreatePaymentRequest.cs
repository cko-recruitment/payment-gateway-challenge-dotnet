namespace PaymentGateway.Api.Models.Requests;

public class CreatePaymentRequest
{
    public string CardNumber { get; set; } = string.Empty;
    public int? ExpiryMonth { get; set; }
    public int? ExpiryYear { get; set; }
    public string Currency { get; set; } = string.Empty;
    public int? Amount { get; set; }
    public string? Cvv { get; set; }
}

public static class CreatePaymentRequestExtensions
{
    extension(CreatePaymentRequest request)
    {
        public PaymentModel ToPayment() =>
            new()
            {
                CardNumber = request.CardNumber ?? throw new ArgumentNullException(nameof(request.CardNumber)),
                ExpiryMonth = request.ExpiryMonth ?? throw new ArgumentNullException(nameof(request.ExpiryMonth)),
                ExpiryYear = request.ExpiryYear ?? throw new ArgumentNullException(nameof(request.ExpiryYear)),
                Currency = request.Currency ?? throw new ArgumentNullException(nameof(request.Currency)),
                Amount = request.Amount ?? throw new ArgumentNullException(nameof(request.Amount)),
                Cvv = request.Cvv ?? throw new ArgumentNullException(nameof(request.Cvv)),
            };

        public PaymentModel ToPayment(BankAuthorization bankAuthorization) => request.ToPayment() with { BankAuthorization = bankAuthorization };
    }
}