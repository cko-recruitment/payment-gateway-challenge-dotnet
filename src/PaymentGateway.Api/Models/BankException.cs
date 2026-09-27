namespace PaymentGateway.Api.Models;

public sealed class BankException(int statusCode, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}
