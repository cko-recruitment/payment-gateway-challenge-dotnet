using PaymentGateway.Api.Interfaces;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services
{
    /// <inheritdoc />
    public class PaymentsHandler(
        IPaymentsRepository paymentsRepository,
        IPaymentRequestValidator paymentRequestValidator
        
      ) : IPaymentsHandler
    {
        /// <inheritdoc />
        public Task<PostPaymentResponse> HandleGetPaymentAsync(Guid id)
        {
            return Task.FromResult(paymentsRepository.Get(id));
        }

        /// <inheritdoc />
        public Task<PostPaymentResponse> HandlePostPaymentAsync(PostPaymentRequest paymentRequest)
        {
            bool valid = paymentRequestValidator.Validate(paymentRequest);
            if (valid)
            {

            }
            return Task.FromResult(new PostPaymentResponse());
        }



    }
}
