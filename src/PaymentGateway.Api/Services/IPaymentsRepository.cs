using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services;

public interface IPaymentsRepository
{
    void Add(PostPaymentResponse payment);
    PostPaymentResponse? Get(Guid id);
}
