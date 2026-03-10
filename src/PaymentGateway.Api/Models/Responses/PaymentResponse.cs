using PaymentGateway.Api.Enums;

namespace PaymentGateway.Api.Models.Responses;

public class PaymentResponse
{
    public required Guid Id { get; set; }
    public required PaymentStatus Status { get; set; }
    public required int CardNumberLastFour { get; set; }
    public required int ExpiryMonth { get; set; }
    public required int ExpiryYear { get; set; }
    public required string Currency { get; set; }
    public required int Amount { get; set; }
}