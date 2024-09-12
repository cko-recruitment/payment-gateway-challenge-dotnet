using PaymentGateway.Api.Constants.Enums;

namespace PaymentGateway.Api.Models.Responses;

public class GetPaymentResponseDto
{
    public Guid Id { get; set; }
    public PaymentStatus Status { get; set; }
    public int CardNumberLastFour { get; set; }
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public string Currency { get; set; }
    public int Amount { get; set; }

    public GetPaymentResponseDto() { }
    public GetPaymentResponseDto(GetPaymentResponse response)
    {
        Id = response.Id;
        Status = response.Status;
        CardNumberLastFour = int.TryParse(response.CardNumber[^4..], out var lastFour) ? lastFour : 0;
        ExpiryMonth = response.ExpiryMonth;
        ExpiryYear = response.ExpiryYear;
        Currency = response.Currency;
        Amount = response.Amount;
    }
}