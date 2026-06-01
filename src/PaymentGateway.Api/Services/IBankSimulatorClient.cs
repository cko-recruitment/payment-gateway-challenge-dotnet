using PaymentGateway.Api.Models.BankSimulator;

namespace PaymentGateway.Api.Services;

public interface IBankSimulatorClient
{
    Task<BankPaymentResponse?> ProcessPaymentAsync(BankPaymentRequest request, CancellationToken cancellationToken);
}
