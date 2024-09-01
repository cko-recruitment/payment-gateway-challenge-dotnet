using System.Text.Json;

using PaymentGateway.Api.Interfaces;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services
{
    /// <inheritdoc />
    public class PaymentsHandler(
        IPaymentsRepository paymentsRepository,
        IPaymentRequestValidator paymentRequestValidator,
        IHttpClientFactory clientFactory) : IPaymentsHandler
    {
        internal static class Url
        {
            public static Uri BankPayment = new Uri("http://localhost:8080/payments");
        };

        /// <inheritdoc />
        public Task<PostPaymentResponse> HandleGetPaymentAsync(Guid id)
        {
            return Task.FromResult(paymentsRepository.Get(id));
        }

        /// <inheritdoc />
        public async Task<PostPaymentResponse> HandlePostPaymentAsync(PostPaymentRequest paymentRequest)
        {
            bool valid = paymentRequestValidator.Validate(paymentRequest, out var validatedRequest);
            if (!valid)
            {
                return CreateRejectedResponse(paymentRequest);
            }

            var bankRequest = new BankRequest
            {
                CardNumber = validatedRequest.CardNumber,
                Amount = validatedRequest.Amount,
                Cvv = validatedRequest.Cvv,
                ExpiryDate = GetBankRequestFormattedDate(validatedRequest.ExpiryMonth, validatedRequest.ExpiryYear),
                Currency = validatedRequest.Currency
            };

            var client = clientFactory.CreateClient();

            string json = JsonSerializer.Serialize(bankRequest);
            var response = await client.PostAsync(
                Url.BankPayment,
                new StringContent(JsonSerializer.Serialize(bankRequest))
            );

            var content = await response.Content.ReadAsStringAsync()!;
            BankResponse? bankResponse = JsonSerializer.Deserialize<BankResponse>(content);

            if (!response.IsSuccessStatusCode || !bankResponse.Authorized)
            {
                return CreateDeclinedResponse(validatedRequest);
            }

            return CreateAuthorizedResponse(validatedRequest);
        }

        private string GetBankRequestFormattedDate(int month, int year)
        {
            return month >= 10 ? $"{month}/{year}" : $"0{month}/{year}";
        }
        private PostPaymentResponse CreateRejectedResponse(PostPaymentRequest paymentRequest)
        {
            var response = new PostPaymentResponseBuilder()
                .WithId(Guid.NewGuid())
                .WithCurrency(paymentRequest.Currency)
                .WithCardNumberLastFour(paymentRequest.CardNumber)
                .WithExpiryMonth(paymentRequest.ExpiryMonth)
                .WithExpiryYear(paymentRequest.ExpiryYear)
                .WithCurrency(paymentRequest.Currency)
                .Rejected()
                .Build();

            paymentsRepository.Add(response);
            return response;
        }
        private PostPaymentResponse CreateAuthorizedResponse(ValidatedPostPaymentRequest paymentRequest)
        {
            var response = new PostPaymentResponseBuilder()
                .WithId(Guid.NewGuid())
                .WithCurrency(paymentRequest.Currency)
                .WithCardNumberLastFour(paymentRequest.CardNumber)
                .WithExpiryMonth(paymentRequest.ExpiryMonth)
                .WithExpiryYear(paymentRequest.ExpiryYear)
                .WithCurrency(paymentRequest.Currency)
                .WithAmount(paymentRequest.Amount)
                .Authorized()
                .Build();

            paymentsRepository.Add(response);
            return response;
        }
        private PostPaymentResponse CreateDeclinedResponse(ValidatedPostPaymentRequest paymentRequest)
        {
            var response = new PostPaymentResponseBuilder()
                .WithId(Guid.NewGuid())
                .WithCurrency(paymentRequest.Currency)
                .WithCardNumberLastFour(paymentRequest.CardNumber)
                .WithExpiryMonth(paymentRequest.ExpiryMonth)
                .WithExpiryYear(paymentRequest.ExpiryYear)
                .WithCurrency(paymentRequest.Currency)
                .WithAmount(paymentRequest.Amount)
                .Declined()
                .Build();

            paymentsRepository.Add(response);
            return response;
        }

    }
}
