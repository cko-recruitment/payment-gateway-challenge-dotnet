using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly ILogger<PaymentRepository> _logger;
    private readonly HttpClient _httpClient;

    public PaymentRepository(ILogger<PaymentRepository> logger, IHttpClientFactory httpClientFactory, string bankUrl)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri(bankUrl);
    }

    public async Task<PostPaymentResult> PostAsync(PostPaymentRequestDto request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/payments", request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();
                return new PostPaymentResult(true, content);
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                return new PostPaymentResult(false, null, error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while processing payment");
            return new PostPaymentResult(false, null, "An unexpected error occurred");
        }
    }
}