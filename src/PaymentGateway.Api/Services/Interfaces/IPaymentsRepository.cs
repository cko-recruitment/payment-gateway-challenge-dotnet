using PaymentGateway.Api.Models;

namespace PaymentGateway.Api.Services.Interfaces;

public interface IPaymentsRepository
{
    void Add(Payment payment);
    Payment Get(Guid id);

    bool Exists(Guid id);
}