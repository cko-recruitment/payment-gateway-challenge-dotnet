
using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Constants.Enums;
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
                var errorMessageTemplate = $"Expiry date of {postPaymentRequest.ExpiryMonth}/{postPaymentRequest.ExpiryYear} is not valid";
                logger.LogError(errorMessageTemplate);
                return CreateRejectedPostPaymentResult(postPaymentRequest, errorMessageTemplate);
            }

            var paymentRequestDto = new PostPaymentRequestDto(postPaymentRequest);
            var response = await paymentsRepository.PostAsync(paymentRequestDto);
            return CreatePostPaymentResult(response.PostToBankResponse, postPaymentRequest);

        }

        private static PostPaymentResult CreatePostPaymentResult(PostToBankResponse postBankResponse, PostPaymentRequest postPaymentRequest)
        {
            var response = new PostPaymentResponse(postPaymentRequest, postBankResponse);
            var result = new PostPaymentResult(false, response);
            if (response.Status == PaymentStatus.Authorized.ToString())
            {
                result.IsSuccess = true;
            }
            return result;
        }

        private static PostPaymentResult CreateRejectedPostPaymentResult(PostPaymentRequest postPaymentRequest, string errorMessage)
        {
            var response = new PostPaymentResponse(postPaymentRequest);
            return new PostPaymentResult(false, response, errorMessage);
        }

        private static bool IsValidExpiryDate(int expiryMonth, int expiryYear)
        {
            return expiryYear == DateTime.Now.Year ? expiryMonth > DateTime.Now.Month : expiryYear > DateTime.Now.Year;
        }
    }
}
