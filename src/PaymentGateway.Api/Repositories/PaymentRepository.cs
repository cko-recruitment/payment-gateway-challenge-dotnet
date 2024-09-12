using System.Net;

using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Models.Results;

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

    public async Task<PostToBankResult> PostAsync(PostPaymentRequestDto request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/payments", request);
            var content = await response.Content.ReadFromJsonAsync<PostToBankResponse>();
            if (response.IsSuccessStatusCode)
            {
                return new PostToBankResult(true, content);
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                return new PostToBankResult(false, content, "Error while processing payment, bank returned error");
            }
            else
            {
                var tryGetError = await response.Content.ReadAsStringAsync();
                return new PostToBankResult(false, null, tryGetError ?? response.ReasonPhrase);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while processing payment");
            return new PostToBankResult(false, null, $"An unexpected error occurred - Error:{ex.Message}");
        }
    }

    public async Task<GetPaymentResult> GetAsync(Guid id)
    {
        try
        {
            // assuming we're interacting with the database using some kind of rest api
            var response = await _httpClient.GetAsync($"/get/payment/{id}");
            var content = await response.Content.ReadFromJsonAsync<GetPaymentResponse>();

            if (response.IsSuccessStatusCode)
            {
                return new GetPaymentResult(content);
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return new GetPaymentResult()
                {
                    ErrorMessage = "Payment not found",
                    IsSuccess = false,
                    GetPaymentResponse = null,
                    StatusCode = HttpStatusCode.NotFound
                };
            }
            return new GetPaymentResult()
            {
                ErrorMessage = $"Error while getting payment Error:{response.ReasonPhrase}",
                IsSuccess = false,
                StatusCode = response.StatusCode
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while getting details for PaymentId:{id}", id);
            return new GetPaymentResult()
            {
                ErrorMessage = $"Unexpected error occurred while getting details for PaymentId:{id} Error:{ex.Message}",
                IsSuccess = false,
                StatusCode = HttpStatusCode.InternalServerError
            };
        }
    }
}