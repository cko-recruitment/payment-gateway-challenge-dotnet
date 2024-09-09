using System.Text.Json.Serialization;

namespace PaymentGateway.Api.Models.Requests
{
    public class PostPaymentRequestDto
    {
        [JsonPropertyName("card_number")]
        public string CardNumber { get; set; }
        [JsonPropertyName("expiry_date")]
        public string ExpiryDate { get; set; }
        [JsonPropertyName("currency")]
        public string Currency { get; set; }
        [JsonPropertyName("amount")]
        public int Amount { get; set; }
        [JsonPropertyName("cvv")]
        public int Cvv { get; set; }

        public PostPaymentRequestDto(PostPaymentRequest request)
        {
            CardNumber = request.CardNumber;
            ExpiryDate = request.ExpiryMonth < 10 ? $"0{request.ExpiryMonth}/{request.ExpiryYear}" : $"{request.ExpiryMonth}/{request.ExpiryYear}";
            Currency = request.Currency;
            Amount = request.Amount;
            Cvv = int.Parse(request.Cvv);
        }
    }
}
