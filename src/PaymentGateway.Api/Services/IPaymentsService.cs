using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services;

public interface IPaymentsService
{
    Task<PaymentProcessingResult> ProcessPaymentAsync(PostPaymentRequest request, CancellationToken cancellationToken = default);
    GetPaymentResponse? GetPayment(Guid id);
}
