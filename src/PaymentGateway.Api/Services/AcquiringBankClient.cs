using System.Net;
using System.Text.Json.Serialization;

using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Services;

public interface IAcquiringBankClient
{
    // In production code, I would use a more robust and intuitive approach such as a discriminated union, or a Result<T> type, but for the sake of this exercise, I will use a nullable string to indicate authorized or not.
    Task<string?> AuthorizeAsync(PaymentModel request, CancellationToken cancellationToken);
}

public class AcquiringBankClient: IAcquiringBankClient
{
    private readonly HttpClient _httpClient;

    public AcquiringBankClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string?> AuthorizeAsync(PaymentModel paymentModel, CancellationToken cancellationToken)
    {
        BankPaymentRequest bankRequest = new(
            CardNumber: paymentModel.CardNumber,
            ExpiryDate: $"{paymentModel.ExpiryMonth:00}/{paymentModel.ExpiryYear:0000}",
            Currency: paymentModel.Currency,
            Amount: paymentModel.Amount,
            Cvv: paymentModel.Cvv);
        
        
        var response = await _httpClient.PostAsJsonAsync(
            "payments",
            bankRequest,
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<BankPaymentResponse>(cancellationToken);
            
            if (result?.Authorized is true) // I'd check and throw an exception for invalid responses in production such as authorized=true but not having an authorization code. 
            {
                return result.AuthorizationCode;
            }
        }

        throw response.StatusCode switch
        {
            HttpStatusCode.BadRequest =>
                new BankException(StatusCodes.Status400BadRequest, "Invalid payment request."),

            HttpStatusCode.ServiceUnavailable =>
                new BankException(StatusCodes.Status503ServiceUnavailable, "Acquiring bank is unavailable."),

            _ =>
                new BankException(StatusCodes.Status500InternalServerError, $"Unexpected response: {response.StatusCode}")
        };
    }

    private sealed record BankPaymentResponse(
        [property: JsonPropertyName("authorized")] bool? Authorized,
        [property: JsonPropertyName("authorized_code")] string? AuthorizationCode
    );

    private sealed record BankPaymentRequest(
        [property: JsonPropertyName("card_number")] string CardNumber,
        [property: JsonPropertyName("expiry_date")] string ExpiryDate,
        [property: JsonPropertyName("currency")] string Currency,
        [property: JsonPropertyName("amount")] int Amount,
        [property: JsonPropertyName("cvv")] string Cvv);
    
}