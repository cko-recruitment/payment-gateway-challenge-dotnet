using PaymentGateway.Api.Interfaces;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services;

/// <inheritdoc />
public class PaymentsRepository : IPaymentsRepository
{
    public List<PostPaymentResponse> Payments = new();

    /// <inheritdoc />
    public void Add(PostPaymentResponse payment)
    {
        Payments.Add(payment);
    }

    /// <inheritdoc />
    public PostPaymentResponse Get(Guid id)
    {
        return Payments.FirstOrDefault(p => p.Id == id);
    }
}