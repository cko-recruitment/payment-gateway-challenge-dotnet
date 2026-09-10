using System.Collections.Concurrent;

using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services;

// Registered as a singleton, so storage must be thread-safe.
public class PaymentsRepository : IPaymentsRepository
{
    private readonly ConcurrentDictionary<Guid, PostPaymentResponse> _payments = new();

    public void Add(PostPaymentResponse payment)
    {
        _payments[payment.Id] = payment;
    }

    public PostPaymentResponse? Get(Guid id)
    {
        return _payments.GetValueOrDefault(id);
    }
}
