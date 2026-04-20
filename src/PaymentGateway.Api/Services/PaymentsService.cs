using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Extensions;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services.Interfaces;

namespace PaymentGateway.Api.Services;

public class PaymentsService : IPaymentService
{
    private const string PaymentUrl = "/payments";
    private readonly HttpClient _httpClient;
    private readonly IPaymentsRepository _paymentsRepository;
    private readonly ILogger<PaymentsService> _logger;

    public PaymentsService(IHttpClientFactory httpClientFactory,
        IPaymentsRepository paymentsRepository,
        ILogger<PaymentsService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("bank");
        _paymentsRepository = paymentsRepository;
        _logger = logger;
    }

    public PaymentResponse Get(Guid id)
    {
        _logger.LogInformation("Retrieving payment with id {PaymentId}", id);
        var payment = _paymentsRepository.Get(id);
        if (payment == null)
        {
            _logger.LogWarning("Payment with id {PaymentId} not found", id);
            throw new KeyNotFoundException($"Payment with id {id} not found.");
        }
        _logger.LogInformation("Payment with id {PaymentId} retrieved successfully", id);
        return payment.ToPaymentResponse();
    }

    public async Task<PaymentResponse> ProcessPaymentAsync(PostPaymentRequest request, Guid idempotencyKey)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object> { { "IdempotencyKey", idempotencyKey } });

        try
        {
            var bankRequest = request.ToBankPaymentRequest();
            var lastFour = bankRequest.CardNumber.Substring(bankRequest.CardNumber.Length - 4, 4);

            _logger.LogInformation("Processing payment for {lastFour} for amount {Amount} {Currency}",
                request.Amount, lastFour, request.Currency);

            var response = await _httpClient.PostAsJsonAsync(PaymentUrl, bankRequest);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var bankResponse = System.Text.Json.JsonSerializer.Deserialize<BankPaymentResponse>(responseBody);

                var status = bankResponse.Authorized ? PaymentStatus.Authorized : PaymentStatus.Declined;
                _logger.LogInformation("Sucessfully called Bank API. Payment status: {PaymentStatus}", status);

                Payment payment = new Payment
                {
                    Id = idempotencyKey,
                    Status = status,
                    CardNumberLastFour = int.Parse(lastFour),
                    ExpiryMonth = request.ExpiryMonth,
                    ExpiryYear = request.ExpiryYear,
                    Currency = request.Currency,
                    Amount = request.Amount,
                    AutorizationCode = bankResponse.AuthorizationCode
                };

                _paymentsRepository.Add(payment);
                _logger.LogInformation("Payment stored in repository");

                return payment.ToPaymentResponse();
            }
            else
            {
                _logger.LogError("Bank API returned error status {StatusCode}",
                    response.StatusCode);
                throw new Exception($"Bank API returned error: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment");
            throw;
        }
    }
}
