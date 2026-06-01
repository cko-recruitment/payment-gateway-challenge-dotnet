using FluentValidation;
using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.BankSimulator;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Repositories;

namespace PaymentGateway.Api.Services;

public class PaymentService : IPaymentService
{
    private readonly IBankSimulatorClient _bankSimulatorClient;
    private readonly IPaymentsRepository _paymentsRepository;
    private readonly IValidator<PostPaymentRequest> _validator;

    public PaymentService(
        IBankSimulatorClient bankSimulatorClient,
        IPaymentsRepository paymentsRepository,
        IValidator<PostPaymentRequest> validator)
    {
        _bankSimulatorClient = bankSimulatorClient;
        _paymentsRepository = paymentsRepository;
        _validator = validator;
    }

    public async Task<ValidatedResponse<PostPaymentResponse>> ProcessPaymentAsync(
        PostPaymentRequest? request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Rejected(new[] { "Payment request is required." });
        }

        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Rejected(validationResult.Errors.Select(error => error.ErrorMessage).ToArray());
        }

        var bankResponse = await _bankSimulatorClient.ProcessPaymentAsync(new BankPaymentRequest
        {
            CardNumber = request.CardNumber,
            ExpiryDate = $"{request.ExpiryMonth:D2}/{request.ExpiryYear}",
            Currency = request.Currency.ToUpperInvariant(),
            Amount = request.Amount,
            Cvv = request.Cvv
        }, cancellationToken);

        if (bankResponse is null)
        {
            return new ValidatedResponse<PostPaymentResponse>
            {
                Status = PaymentServiceResultStatus.Unavailable
            };
        }

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            Status = bankResponse.Authorized ? PaymentStatus.Authorized : PaymentStatus.Declined,
            CardNumberLastFour = request.CardNumber[^4..],
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            Currency = request.Currency.ToUpperInvariant(),
            Amount = request.Amount
        };

        _paymentsRepository.Add(payment);

        return new ValidatedResponse<PostPaymentResponse>
        {
            Status = PaymentServiceResultStatus.Created,
            Result = new PostPaymentResponse
            {
                Id = payment.Id,
                Status = payment.Status,
                CardNumberLastFour = payment.CardNumberLastFour,
                ExpiryMonth = payment.ExpiryMonth,
                ExpiryYear = payment.ExpiryYear,
                Currency = payment.Currency,
                Amount = payment.Amount
            }
        };
    }

    public ValidatedResponse<GetPaymentResponse> GetPayment(Guid id)
    {
        var payment = _paymentsRepository.Get(id);
        if (payment is null)
        {
            return new ValidatedResponse<GetPaymentResponse>
            {
                Status = PaymentServiceResultStatus.NotFound
            };
        }

        return new ValidatedResponse<GetPaymentResponse>
        {
            Status = PaymentServiceResultStatus.Found,
            Result = new GetPaymentResponse
            {
                Id = payment.Id,
                Status = payment.Status,
                CardNumberLastFour = payment.CardNumberLastFour,
                ExpiryMonth = payment.ExpiryMonth,
                ExpiryYear = payment.ExpiryYear,
                Currency = payment.Currency,
                Amount = payment.Amount
            }
        };
    }

    private static ValidatedResponse<PostPaymentResponse> Rejected(IReadOnlyCollection<string> errors)
    {
        return new ValidatedResponse<PostPaymentResponse>
        {
            Status = PaymentServiceResultStatus.Rejected,
            Result = new PostPaymentResponse
            {
                Status = PaymentStatus.Rejected,
                Errors = errors
            }
        };
    }
}
