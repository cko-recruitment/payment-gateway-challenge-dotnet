using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services;

public interface IPaymentService
{
    Task<ValidatedResponse<PostPaymentResponse>> ProcessPaymentAsync(
        PostPaymentRequest? request,
        CancellationToken cancellationToken);

    ValidatedResponse<GetPaymentResponse> GetPayment(Guid id);
}
