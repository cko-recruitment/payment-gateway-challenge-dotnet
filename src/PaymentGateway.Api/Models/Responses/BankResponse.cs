using System.Text.Json.Serialization;

using PaymentGateway.Api.Converters;

namespace PaymentGateway.Api.Models.Responses
{
    public class BankResponse
    {
        [JsonPropertyName("authorized")]
        public bool Authorized { get; set; }

        [JsonPropertyName("authorization_code")]
        [JsonConverter(typeof(GuidJsonConverter))]
        public Guid AuthorizationCode {  get; set; }
    }
}
