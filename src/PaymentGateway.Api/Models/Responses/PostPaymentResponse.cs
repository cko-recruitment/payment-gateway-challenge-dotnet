using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Models.Responses;

public class PostPaymentResponse
{
    public Guid Id { get; set; }
    public string Status { get; set; }
    public int CardNumberLastFour { get; set; }
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public string Currency { get; set; }
    public int Amount { get; set; }

    public PostPaymentResponse(PostPaymentRequest postPaymentRequest, string status)
    {
        Id = Guid.NewGuid();
        Status = status; // authorized or declined
        CardNumberLastFour = int.TryParse(postPaymentRequest.CardNumber[^4..], out var lastFour) ? lastFour : 0;
        ExpiryMonth = postPaymentRequest.ExpiryMonth;
        ExpiryYear = postPaymentRequest.ExpiryYear;
        Currency = postPaymentRequest.Currency;
        Amount = postPaymentRequest.Amount;
    }

    public PostPaymentResponse() { }
}