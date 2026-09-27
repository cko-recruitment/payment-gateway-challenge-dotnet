using PaymentGateway.Api.Enums;

namespace PaymentGateway.Api.Models;


public record BankAuthorization(PaymentStatus Status, string? AuthorizationCode = null);
public record PaymentModel
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string CardNumber { get; init; }

    public int ExpiryMonth { get; init; }

    public int ExpiryYear { get; init; }

    public required string Currency { get; init; }

    public required int Amount { get; init; }

    public required string Cvv { get; init; }

    public BankAuthorization BankAuthorization { get; init; } = new(PaymentStatus.Pending);
}