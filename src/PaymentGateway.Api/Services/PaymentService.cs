using PaymentGateway.Api.Constants.Enums;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Models.Results;
using PaymentGateway.Api.Repositories;

namespace PaymentGateway.Api.Services
{
    public class PaymentService(ILogger<PaymentService> logger, IPaymentRepository paymentsRepository) : IPaymentService
    {
        public async Task<PostPaymentResult> PostPaymentAsync(PostPaymentRequest postPaymentRequest)
        {
            var validExpiryDate = IsValidExpiryDate(postPaymentRequest.ExpiryMonth, postPaymentRequest.ExpiryYear);
            if (!validExpiryDate)
            {
                var errorMessageTemplate = $"Expiry date of {postPaymentRequest.ExpiryMonth}/{postPaymentRequest.ExpiryYear} is not valid";
                logger.LogError(errorMessageTemplate);
                return CreatePostPaymentResult(postPaymentRequest, PaymentStatus.Rejected.ToString(), false, errorMessageTemplate);
            }

            var paymentRequestDto = new PostPaymentRequestDto(postPaymentRequest);
            var response = await paymentsRepository.PostAsync(paymentRequestDto);
            return response.PostToBankResponse.Authorized
                ? CreatePostPaymentResult(postPaymentRequest, PaymentStatus.Authorized.ToString(), response.IsSuccess)
                : CreatePostPaymentResult(postPaymentRequest, PaymentStatus.Declined.ToString(), response.IsSuccess);
        }

        public async Task<GetPaymentResult> GetPaymentByIdAsync(Guid id)
        {
            var response = await paymentsRepository.GetAsync(id);
            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                logger.LogError(response.ErrorMessage);
                return response;
            }
            return response;
        }

        private static PostPaymentResult CreatePostPaymentResult(PostPaymentRequest postPaymentRequest, string status, bool isSuccess, string errorMessage = null)
        {
            var response = new PostPaymentResponse(postPaymentRequest, status);
            var result = new PostPaymentResult(isSuccess, response, errorMessage);
            return result;
        }


        private static bool IsValidExpiryDate(int expiryMonth, int expiryYear)
        {
            return expiryYear == DateTime.Now.Year ? expiryMonth > DateTime.Now.Month : expiryYear > DateTime.Now.Year;
        }
    }
}

