using PaymentGateway.Api.Models.Domain;
using PaymentGateway.Api.Repositories.Payment;

namespace PaymentGateway.Api.Services;

public class PaymentsRepository : IPaymentRepository
{
    private readonly Dictionary<Guid, Payment> Payments = new();

    private readonly Dictionary<string, (Payment Payment, DateTime CreatedAt)> IdempotencyStore = new();

    public void Add(Payment payment)
    {
        Payments[payment.Id] = payment;
        if (!string.IsNullOrEmpty(payment.IdempotencyKey) && !IdempotencyStore.ContainsKey(payment.IdempotencyKey))
        {
            IdempotencyStore[payment.IdempotencyKey] = (payment, DateTime.UtcNow);
        }
    }

    public Payment? Get(Guid paymentId)
    {
        Payments.TryGetValue(paymentId, out var payment);
        return payment;
    }

    public Payment? GetByIdempotencyKey(string idempotencyKey)
    {
        if (IdempotencyStore.TryGetValue(idempotencyKey, out var entry))
        {
            if (DateTime.UtcNow - entry.CreatedAt < TimeSpan.FromHours(24))
            {
                return entry.Payment;
            }

            IdempotencyStore.Remove(idempotencyKey);
        }

        return null;
    }
}