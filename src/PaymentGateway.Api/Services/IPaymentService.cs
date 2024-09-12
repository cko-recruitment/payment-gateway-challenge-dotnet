using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Models.Results;

namespace PaymentGateway.Api.Services
{
    public interface IPaymentService
    {
        Task<PostPaymentResult> PostPaymentAsync(PostPaymentRequest postPaymentRequest);
        Task<GetPaymentResult> GetPaymentByIdAsync(Guid id);
    }
}
