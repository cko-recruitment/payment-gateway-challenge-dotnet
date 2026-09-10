using System.Net.Http.Json;

using PaymentGateway.Api.Models.Bank;

namespace PaymentGateway.Api.Services;

// Talks to the acquiring bank simulator. Any non-success response or transport
// failure is treated as the bank being unavailable rather than a declined payment.
public class BankClient : IBankClient
{
    private readonly HttpClient _httpClient;

    public BankClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<BankPaymentResult> ProcessPaymentAsync(BankPaymentRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/payments", request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return BankPaymentResult.Unavailable();
            }

            var body = await response.Content.ReadFromJsonAsync<BankPaymentResponse>(cancellationToken: cancellationToken);
            return BankPaymentResult.Success(body?.Authorized ?? false, body?.AuthorizationCode);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return BankPaymentResult.Unavailable();
        }
    }
}
