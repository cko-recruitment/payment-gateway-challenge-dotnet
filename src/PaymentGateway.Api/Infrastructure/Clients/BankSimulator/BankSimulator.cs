using System.Net;

using FluentResults;

using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Infrastructure.Clients.BankSimulator;

public class BankSimulator(HttpClient httpClient, ILogger<BankSimulator> logger) : IBankSimulator
{
    public async Task<Result<BankSimulatorResponse>> ProcessPaymentAsync(BankSimulatorRequest request,
        Guid correlationId)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("payments", request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<BankSimulatorResponse>();

                if (result == null)
                {
                    logger.LogError("Bank simulator returned success but body was empty or invalid.");
                    return Result.Fail("Failed to deserialize bank simulator response");
                }

                logger.LogInformation("Bank simulator returned success");
                return Result.Ok(result);
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var error = await response.Content.ReadAsStringAsync();

                logger.LogWarning("Bank simulator returned 400 Bad Request: {Error}", error);

                return Result.Fail($"Bank simulator: Bad Request - {error}");
            }

            if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                logger.LogError("Bank simulator: Service Unavailable");

                return Result.Fail("Bank simulator: Service Unavailable");
            }

            logger.LogError("Bank simulator returned unexpected status code: {StatusCode}", response.StatusCode);

            return Result.Fail($"Bank simulator: Unexpected error ({response.StatusCode})");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calling bank simulator: {CorrelationId}", correlationId);

            return Result.Fail("Bank simulator: Service Unavailable");
        }
    }
}