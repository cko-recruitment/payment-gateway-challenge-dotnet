namespace PaymentGateway.Api.Models.Requests;

public static class CreatePaymentRequestExtensions
{
    extension(CreatePaymentRequest request)
    {
        public BankPaymentRequest ToBankPaymentRequest()
        {
            int expiryMonth = request.ExpiryMonth ?? throw new ArgumentNullException(nameof(request.ExpiryMonth));
            int expiryYear = request.ExpiryYear ?? throw new ArgumentNullException(nameof(request.ExpiryYear));

            return new(
                CardNumber: request.CardNumber ?? throw new ArgumentNullException(nameof(request.CardNumber)),
                ExpiryDate: $"{expiryMonth:00}/{expiryYear:0000}",
                Currency: request.Currency ?? throw new ArgumentNullException(nameof(request.Currency)),
                Amount: request.Amount ?? throw new ArgumentNullException(nameof(request.Amount)),
                Cvv: request.Cvv ?? throw new ArgumentNullException(nameof(request.Cvv)));
        }

        public PaymentModel ToPaymentModel(BankAuthorization bankAuthorization) =>
            new()
            {
                CardNumber = request.CardNumber ?? throw new ArgumentNullException(nameof(request.CardNumber)),
                ExpiryMonth = request.ExpiryMonth ?? throw new ArgumentNullException(nameof(request.ExpiryMonth)),
                ExpiryYear = request.ExpiryYear ?? throw new ArgumentNullException(nameof(request.ExpiryYear)),
                Currency = request.Currency ?? throw new ArgumentNullException(nameof(request.Currency)),
                Amount = request.Amount ?? throw new ArgumentNullException(nameof(request.Amount)),
                BankAuthorization = bankAuthorization
            };
    }
}