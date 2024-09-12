using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Models.Results;

namespace PaymentGateway.Api.Repositories
{
    public interface IPaymentRepository
    {
        Task<PostToBankResult> PostAsync(PostPaymentRequestDto request);
        Task<GetPaymentResult> GetAsync(Guid id);
    }
}
