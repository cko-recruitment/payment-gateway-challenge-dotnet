using System.Text.Json.Serialization;

namespace PaymentGateway.Api.Models.Responses;

public class PostPaymentResponse
{
    public bool Authorized { get; set; }
    [JsonPropertyName("authorization_code")]
    public string AuthorizationCode { get; set; }
}
