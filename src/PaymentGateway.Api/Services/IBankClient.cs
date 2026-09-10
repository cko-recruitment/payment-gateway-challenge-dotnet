using PaymentGateway.Api.Models.Bank;

namespace PaymentGateway.Api.Services;

public interface IBankClient
{
    Task<BankPaymentResult> ProcessPaymentAsync(BankPaymentRequest request, CancellationToken cancellationToken = default);
}
