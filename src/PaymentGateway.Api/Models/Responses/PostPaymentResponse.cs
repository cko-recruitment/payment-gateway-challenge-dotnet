using PaymentGateway.Api.Constants.Enums;
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

    public PostPaymentResponse(PostPaymentRequest postPaymentRequest, PostToBankResponse postToBankResponse)
    {
        Id = Guid.NewGuid();
        Status = postToBankResponse.Authorized ? PaymentStatus.Authorized.ToString() : PaymentStatus.Declined.ToString();
        CardNumberLastFour = int.TryParse(postPaymentRequest.CardNumber[^4..], out var lastFour) ? lastFour : 0;
        ExpiryMonth = postPaymentRequest.ExpiryMonth;
        ExpiryYear = postPaymentRequest.ExpiryYear;
        Currency = postPaymentRequest.Currency;
        Amount = postPaymentRequest.Amount;
    }

    public PostPaymentResponse(PostPaymentRequest postPaymentRequest)
    {
        Id = Guid.NewGuid();
        Status = PaymentStatus.Rejected.ToString();
        CardNumberLastFour = int.TryParse(postPaymentRequest.CardNumber[^4..], out var lastFour) ? lastFour : 0;
        ExpiryMonth = postPaymentRequest.ExpiryMonth;
        ExpiryYear = postPaymentRequest.ExpiryYear;
        Currency = postPaymentRequest.Currency;
        Amount = postPaymentRequest.Amount;
    }

    public PostPaymentResponse() { }
}