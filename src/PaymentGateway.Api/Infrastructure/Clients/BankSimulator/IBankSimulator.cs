using FluentResults;

using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Infrastructure.Clients.BankSimulator;

public interface IBankSimulator
{
    Task<Result<BankSimulatorResponse>> ProcessPaymentAsync(BankSimulatorRequest request, Guid correlationId);
}