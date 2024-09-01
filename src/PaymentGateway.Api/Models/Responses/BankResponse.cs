using System.Text.Json.Serialization;

namespace PaymentGateway.Api.Models.Responses
{
    public class BankResponse
    {
        [JsonPropertyName("authorized")]
        public bool Authorized { get; set; }

        [JsonPropertyName("authorization_code")]
        public Guid AuthorizationCode {  get; set; }
    }
}
