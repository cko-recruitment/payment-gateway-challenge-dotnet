namespace PaymentGateway.Api.Models.Bank;

// Wraps the outcome of calling the acquiring bank, distinguishing a reachable
// bank (authorized/declined) from one that could not be reached or errored.
public class BankPaymentResult
{
    public bool IsSuccess { get; init; }
    public bool Authorized { get; init; }
    public string? AuthorizationCode { get; init; }

    public static BankPaymentResult Unavailable() => new() { IsSuccess = false };

    public static BankPaymentResult Success(bool authorized, string? authorizationCode) => new()
    {
        IsSuccess = true,
        Authorized = authorized,
        AuthorizationCode = authorizationCode
    };
}
