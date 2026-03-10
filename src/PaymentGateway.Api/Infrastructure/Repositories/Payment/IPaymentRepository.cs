using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Repositories.Payment;

public interface IPaymentRepository
{
    void Add(Models.Domain.Payment payment);
    Models.Domain.Payment? Get(Guid paymentId);
    Models.Domain.Payment? GetByIdempotencyKey(string idempotencyKey);
}