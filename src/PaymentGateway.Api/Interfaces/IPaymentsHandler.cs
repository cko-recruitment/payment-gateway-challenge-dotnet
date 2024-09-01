using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Interfaces
{
    public interface IPaymentsHandler
    {
        /// <summary>
        /// Handles a GetPaymentRequest
        /// </summary>
        /// <param name="paymentId">The payment id</param>
        Task<PostPaymentResponse> HandleGetPaymentAsync(Guid paymentId);

        /// <summary>
        /// Handles a PostPaymentRequest
        /// </summary>
        /// <param name="paymentId">The payment id</param>
        Task<PostPaymentResponse> HandlePostPaymentAsync(PostPaymentRequest paymentRequest);
    }
}
