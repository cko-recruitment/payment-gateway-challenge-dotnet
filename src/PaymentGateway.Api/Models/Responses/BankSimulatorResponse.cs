using System.Text.Json.Serialization;

namespace PaymentGateway.Api.Models.Responses;

public class BankSimulatorResponse
{
    [JsonPropertyName("authorized")] public required bool Authorized { get; set; }

    [JsonPropertyName("authorization_code")]
    public required string AuthorizationCode { get; set; }
}