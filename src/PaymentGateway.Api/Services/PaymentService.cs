
using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Repositories;

namespace PaymentGateway.Api.Services
{
    public class PaymentService(ILogger<PaymentService> logger, IPaymentRepository paymentsRepository) : IPaymentService
    {
        public async Task<PostPaymentResult> ProcessPaymentAsync(PostPaymentRequest postPaymentRequest)
        {
            var validExpiryDate = IsValidExpiryDate(postPaymentRequest.ExpiryMonth, postPaymentRequest.ExpiryYear);
            if (!validExpiryDate)
            {
                logger.LogError("Expiry date of {month}/{year} is not valid", postPaymentRequest.ExpiryMonth, postPaymentRequest.ExpiryYear);
                return null;
            }

            var paymentRequestDto = new PostPaymentRequestDto(postPaymentRequest);
            var response = await paymentsRepository.PostAsync(paymentRequestDto);
            return response;
        }

        private static bool IsValidExpiryDate(int expiryMonth, int expiryYear)
        {
            return expiryYear == DateTime.Now.Year ? expiryMonth > DateTime.Now.Month : expiryYear > DateTime.Now.Year;
        }
    }
}
