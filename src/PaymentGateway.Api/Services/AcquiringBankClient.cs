using System.Net;
using System.Text.Json;
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
        
        
        using var response = await _httpClient.PostAsJsonAsync(
            "payments",
            bankRequest,
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<BankPaymentResponse>(cancellationToken);

            return result switch
            {
                { Authorized: false } => null,
                { Authorized: true } when !string.IsNullOrWhiteSpace(result.AuthorizationCode) => result.AuthorizationCode,
                _ => throw new BankException(StatusCodes.Status500InternalServerError,
                    "Invalid response from acquiring bank.")
            };
        }

        ErrorBody? error = null;
        if( response.StatusCode == HttpStatusCode.BadRequest)
        {
            error = await response.Content.ReadFromJsonAsync<ErrorBody>(cancellationToken);
        }
        
        throw new BankException((int)response.StatusCode, error?.GetMessage() ?? string.Empty);
        
        // In production, transport-level failures (e.g. timeouts and connection errors) could be translated into
        // bank-specific exceptions by an HttpClient delegating handler (in Program.cs): 
        // services
        //      .AddHttpClient<IAcquiringBankClient, AcquiringBankClient>()
        //      .AddHttpMessageHandler<BankErrorHandler>();
    }

    private sealed record BankPaymentResponse(
        [property: JsonPropertyName("authorized")] bool? Authorized,
        [property: JsonPropertyName("authorization_code")] string? AuthorizationCode
    );

    private sealed record ErrorBody(
        [property: JsonPropertyName("error_message")] string? Messages,
        [property: JsonPropertyName("errorMessage")] string? OtherMessages
    )
    {
        public string GetMessage() => Messages ?? OtherMessages ?? string.Empty;
    }

    private sealed record BankPaymentRequest(
        [property: JsonPropertyName("card_number")] string CardNumber,
        [property: JsonPropertyName("expiry_date")] string ExpiryDate,
        [property: JsonPropertyName("currency")] string Currency,
        [property: JsonPropertyName("amount")] int Amount,
        [property: JsonPropertyName("cvv")] string Cvv);
    
}
