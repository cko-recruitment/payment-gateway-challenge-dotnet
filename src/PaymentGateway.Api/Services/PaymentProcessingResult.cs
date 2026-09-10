using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services;

// Result of attempting to process a payment: either a bank-unreachable failure,
// or a response representing an Authorized/Declined outcome.
public class PaymentProcessingResult
{
    public bool BankUnavailable { get; init; }
    public PostPaymentResponse? Response { get; init; }

    public static PaymentProcessingResult Unavailable() => new() { BankUnavailable = true };

    public static PaymentProcessingResult Success(PostPaymentResponse response) => new() { Response = response };
}
