namespace PaymentGateway.Api.Models.Requests;

public class PostPaymentRequest
{
    public required string CardNumber { get; set; }
    public required int ExpiryMonth { get; set; }
    public required int ExpiryYear { get; set; }
    public required string Currency { get; set; }
    public required int Amount { get; set; }
    public required string CVV { get; set; }
}