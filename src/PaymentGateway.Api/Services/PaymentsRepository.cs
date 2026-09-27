using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Services;

public class PaymentsRepository
{
    private readonly List<PaymentModel> _payments = [];

    public IReadOnlyList<PaymentModel> Payments => _payments.AsReadOnly();

    public void Add(PaymentModel paymentModel) => _payments.Add(paymentModel);

    public PaymentModel? Get(Guid id) => _payments.FirstOrDefault(p => p.Id == id);
}