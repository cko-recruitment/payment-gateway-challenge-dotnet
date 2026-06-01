using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using PaymentGateway.Api.Models.BankSimulator;

namespace PaymentGateway.Api.Services;

public class BankSimulatorClient : IBankSimulatorClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BankSimulatorClient> _logger;

    public BankSimulatorClient(
        HttpClient httpClient,
        ILogger<BankSimulatorClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<BankPaymentResponse?> ProcessPaymentAsync(
        BankPaymentRequest request,
        CancellationToken cancellationToken)
    {
        HttpResponseMessage response;
        try
        {
            var json = JsonSerializer.Serialize(request);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            response = await _httpClient.PostAsync("/payments", content, cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogWarning(exception, "Bank simulator request failed.");
            return null;
        }
        catch (TaskCanceledException exception)
        {
            _logger.LogWarning(exception, "Bank simulator request timed out.");
            return null;
        }

        _logger.LogInformation("Bank simulator returned status code {StatusCode}.", response.StatusCode);

        if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<BankPaymentResponse>(cancellationToken);
    }
}
