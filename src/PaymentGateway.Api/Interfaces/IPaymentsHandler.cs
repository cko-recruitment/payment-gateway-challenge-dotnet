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
        /// <returns>a stored <see cref="PostPaymentRequest"/></returns>
        Task<PostPaymentResponse> HandleGetPaymentAsync(Guid paymentId);

        /// <summary>
        /// Handles a PostPaymentRequest
        /// </summary>
        /// <param name="paymentRequest">The payment request</param>
        /// <returns>a <see cref="PostPaymentResponse"/></returns>
        Task<PostPaymentResponse> HandlePostPaymentAsync(PostPaymentRequest paymentRequest);
    }
}
