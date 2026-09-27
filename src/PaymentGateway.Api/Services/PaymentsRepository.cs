using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Services;

public class PaymentsRepository
{
    private readonly List<Payment> _payments = [];
    
    public IReadOnlyList<Payment> Payments => _payments.AsReadOnly();
    
    public void Add(Payment payment) => _payments.Add(payment);

    public Payment? Get(Guid id) => _payments.FirstOrDefault(p => p.Id == id);
}