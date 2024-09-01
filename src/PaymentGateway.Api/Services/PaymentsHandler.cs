using System.Text.Json;

using PaymentGateway.Api.Interfaces;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services
{
    /// <inheritdoc />
    public class PaymentsHandler(
        IPaymentsRepository paymentsRepository,
        IPaymentRequestValidator paymentRequestValidator) : IPaymentsHandler
    {
        private static readonly HttpClient client = new HttpClient();

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

            string json = JsonSerializer.Serialize(bankRequest);
            var response = await client.PostAsync(
                Url.BankPayment,
                new StringContent(JsonSerializer.Serialize(bankRequest))
            );

            var cont = await response.Content.ReadAsStringAsync()!;
            BankResponse? bankResponse = JsonSerializer.Deserialize<BankResponse>(cont);

            if (!response.IsSuccessStatusCode || !bankResponse.Authorized)
            {
                return CreateDeclinedResponse(paymentRequest);
            }

            return CreateAuthorizedResponse(paymentRequest);
        }

        private string GetBankRequestFormattedDate(int month, int year)
        {
            return month >= 10 ? $"{month}/{year}" : $"0{month}/{year}";
        }

        private PostPaymentResponse CreateRejectedResponse(PostPaymentRequest paymentRequest)
        {
            var response = new PostPaymentResponseBuilder()
                .WithNewId()
                .WithStatus(PaymentStatus.Rejected)
                .WithCurrency(paymentRequest.Currency)
                .WithCardNumberLastFour(paymentRequest.CardNumber)
                .WithExpiryMonth(paymentRequest.ExpiryMonth)
                .WithExpiryYear(paymentRequest.ExpiryYear)
                .WithCurrency(paymentRequest.Currency)
                .Build();

            paymentsRepository.Add(response);
            return response;
        }
        private PostPaymentResponse CreateAuthorizedResponse(PostPaymentRequest paymentRequest)
        {
            var response = new PostPaymentResponseBuilder()
                .WithNewId()
                .WithStatus(PaymentStatus.Authorized)
                .WithCurrency(paymentRequest.Currency)
                .WithCardNumberLastFour(paymentRequest.CardNumber)
                .WithExpiryMonth(paymentRequest.ExpiryMonth)
                .WithExpiryYear(paymentRequest.ExpiryYear)
                .WithCurrency(paymentRequest.Currency)
                .Build();

            paymentsRepository.Add(response);
            return response;
        }
        private PostPaymentResponse CreateDeclinedResponse(PostPaymentRequest paymentRequest)
        {
            var response = new PostPaymentResponseBuilder()
                .WithNewId()
                .WithStatus(PaymentStatus.Declined)
                .WithCurrency(paymentRequest.Currency)
                .WithCardNumberLastFour(paymentRequest.CardNumber)
                .WithExpiryMonth(paymentRequest.ExpiryMonth)
                .WithExpiryYear(paymentRequest.ExpiryYear)
                .WithCurrency(paymentRequest.Currency)
                .Build();

            paymentsRepository.Add(response);
            return response;
        }

    }
}
