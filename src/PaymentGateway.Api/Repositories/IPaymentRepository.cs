using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Repositories
{
    public interface IPaymentRepository
    {
        Task<PostToBankResponseResult> PostAsync(PostPaymentRequestDto request);
    }
}
