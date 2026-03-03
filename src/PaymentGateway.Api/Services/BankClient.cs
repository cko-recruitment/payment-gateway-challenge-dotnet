using PaymentGateway.Api.Models.Bank;
using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Services;

public interface IBankClient
{
    Task<BankPaymentResponse?> ProcessPaymentAsync(PostPaymentRequest request);
}

public class BankClient : IBankClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BankClient> _logger;

    public BankClient(HttpClient httpClient, ILogger<BankClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<BankPaymentResponse?> ProcessPaymentAsync(PostPaymentRequest request)
    {
        try
        {
            var bankRequest = new BankPaymentRequest
            {
                CardNumber = request.CardNumber,
                ExpiryDate = $"{request.ExpiryMonth:D2}/{request.ExpiryYear}",
                Currency = request.Currency,
                Amount = request.Amount,
                Cvv = request.Cvv
            };

            var response = await _httpClient.PostAsJsonAsync("/payments", bankRequest);

            if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            {
                _logger.LogWarning("Bank service unavailable");
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Bank API returned error status code: {StatusCode}", response.StatusCode);
                return null;
            }

            var bankResponse = await response.Content.ReadFromJsonAsync<BankPaymentResponse>();
            return bankResponse;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error calling bank API");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing payment with bank");
            return null;
        }
    }
}