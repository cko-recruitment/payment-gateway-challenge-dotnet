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

    public async Task<PostToBankResponseResult> PostAsync(PostPaymentRequestDto request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/payments", request);
            var content = await response.Content.ReadFromJsonAsync<PostToBankResponse>();
            if (response.IsSuccessStatusCode)
            {
                return new PostToBankResponseResult(true, content);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                return new PostToBankResponseResult(false, content, "Error while processing payment, bank returned error.");
            }
            else
            {
                var tryGetError = await response.Content.ReadAsStringAsync();
                return new PostToBankResponseResult(false, null, tryGetError);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while processing payment");
            return new PostToBankResponseResult(false, null, $"An unexpected error occurred - Error:{ex.Message}");
        }
    }
}