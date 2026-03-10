using FluentResults;

using PaymentGateway.Api.Common.Extensions;
using PaymentGateway.Api.Common.GuidGenerator;
using PaymentGateway.Api.Common.Mapping;
using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Infrastructure.Clients.BankSimulator;
using PaymentGateway.Api.Models.Domain;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Repositories.Payment;
using PaymentGateway.Api.Validation;

namespace PaymentGateway.Api.Services;

public class PaymentService(
    ILogger<PaymentService> logger,
    IPaymentRepository paymentRepository,
    IBankSimulator bankSimulator,
    IGuidGenerator guidGenerator
)
{
    public async Task<Result<PaymentResponse>> ProcessPaymentAsync(PostPaymentRequest request, string? idempotencyKey)
    {
        if (!string.IsNullOrEmpty(idempotencyKey))
        {
            var existing = paymentRepository.GetByIdempotencyKey(idempotencyKey);
            if (existing is not null)
            {
                logger.LogWarning("Payment with idempotency key already exists: {IdempotencyKey}", idempotencyKey);

                return Result.Fail(new PaymentError(PaymentErrorType.Conflict,
                    $"A payment with this idempotency key already exists: {idempotencyKey}"));
            }
        }

        var validationResult = await new PostPaymentRequestValidator().ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            logger.LogWarning("Payment validation failed: {Errors}", validationResult.Errors);
            return Result.Fail<PaymentResponse>(new PaymentError(PaymentErrorType.ValidationFailed,
                validationResult.Errors));
        }

        var correlationId = guidGenerator.NewGuid();

        logger.LogInformation("Payment validation successful: {CorrelationId}", correlationId.ToString());

        var bankResult = await bankSimulator.ProcessPaymentAsync(request.ToBankSimulatorRequest(), correlationId);

        if (bankResult.IsFailed)
        {
            logger.LogError("Bank payment processing failed: {Error}", bankResult.ToPaymentError());
            return Result.Fail<PaymentResponse>(bankResult.ToPaymentError());
        }

        var status = bankResult.Value.Authorized ? PaymentStatus.Authorized : PaymentStatus.Declined;
        logger.LogInformation("Payment processed with status: {Status}", status);

        return Result.Ok(CreateAndStorePayment(request, status, correlationId, bankResult.Value.AuthorizationCode,
                idempotencyKey)
            .ToPaymentResponse());
    }

    private Payment CreateAndStorePayment(PostPaymentRequest request, PaymentStatus status, Guid correlationId,
        string authorizationCode, string? idempotencyKey)
    {
        var payment = new Payment
        {
            Id = correlationId,
            Status = status,
            CardNumberLastFour = request.GetCardNumberLastFour(),
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            Currency = request.Currency,
            Amount = request.Amount,
            AuthorizationCode = string.IsNullOrEmpty(authorizationCode) ? null : authorizationCode,
            IdempotencyKey = idempotencyKey
        };


        paymentRepository.Add(payment);
        logger.LogInformation("Payment saved with PaymentID: {PaymentId}", payment.Id);

        return payment;
    }

    public Result<PaymentResponse> GetPayment(Guid paymentId)
    {
        var payment = paymentRepository.Get(paymentId);

        if (payment == null)
        {
            logger.LogWarning("Payment not found: {PaymentId}", paymentId.ToString());
            return Result.Fail<PaymentResponse>(new PaymentError(PaymentErrorType.NotFound, "Payment not found"));
        }

        logger.LogInformation("Payment retrieved: {PaymentId}", paymentId.ToString());
        return Result.Ok(payment.ToPaymentResponse());
    }
}