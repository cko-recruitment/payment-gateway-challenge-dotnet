using PaymentGateway.Api.Models;
using PaymentGateway.Api.Services.Interfaces;

namespace PaymentGateway.Api.Services;

public class PaymentsRepository : IPaymentsRepository
{
    public List<Payment> Payments = new();
    
    public void Add(Payment payment)
    {
        Payments.Add(payment);
    }

    public bool Exists(Guid id)
    {
        return Payments.Any(p => p.Id == id);
    }

    public Payment Get(Guid id)
    {
        var payment = Payments.FirstOrDefault(p => p.Id == id);

        return payment;
    }
}
