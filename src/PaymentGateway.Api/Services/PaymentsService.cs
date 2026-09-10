using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Models.Bank;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services;

public class PaymentsService : IPaymentsService
{
    private readonly IBankClient _bankClient;
    private readonly IPaymentsRepository _paymentsRepository;

    public PaymentsService(IBankClient bankClient, IPaymentsRepository paymentsRepository)
    {
        _bankClient = bankClient;
        _paymentsRepository = paymentsRepository;
    }

    public async Task<PaymentProcessingResult> ProcessPaymentAsync(PostPaymentRequest request, CancellationToken cancellationToken = default)
    {
        var bankRequest = new BankPaymentRequest
        {
            CardNumber = request.CardNumber,
            ExpiryDate = $"{request.ExpiryMonth:D2}/{request.ExpiryYear}",
            Currency = request.Currency,
            Amount = request.Amount,
            Cvv = request.Cvv
        };

        var bankResult = await _bankClient.ProcessPaymentAsync(bankRequest, cancellationToken);
        if (!bankResult.IsSuccess)
        {
            return PaymentProcessingResult.Unavailable();
        }

        var payment = new PostPaymentResponse
        {
            Id = Guid.NewGuid(),
            Status = bankResult.Authorized ? PaymentStatus.Authorized : PaymentStatus.Declined,
            CardNumberLastFour = request.CardNumber[^4..],
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            Currency = request.Currency,
            Amount = request.Amount
        };

        _paymentsRepository.Add(payment);

        return PaymentProcessingResult.Success(payment);
    }

    public GetPaymentResponse? GetPayment(Guid id)
    {
        var payment = _paymentsRepository.Get(id);
        if (payment is null)
        {
            return null;
        }

        return new GetPaymentResponse
        {
            Id = payment.Id,
            Status = payment.Status,
            CardNumberLastFour = payment.CardNumberLastFour,
            ExpiryMonth = payment.ExpiryMonth,
            ExpiryYear = payment.ExpiryYear,
            Currency = payment.Currency,
            Amount = payment.Amount
        };
    }
}
