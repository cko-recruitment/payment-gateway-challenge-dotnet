using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Interfaces
{
    public interface IPaymentRequestValidator
    {
        /// <summary>
        /// Validates a payment request
        /// </summary>
        /// <param name="request">The <see cref="PostPaymentRequest"/> to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        bool Validate(PostPaymentRequest request, out ValidatedPostPaymentRequest validatedPostPayment);
    }
}
