using FluentResults;

using FluentValidation.Results;

using PaymentGateway.Api.Enums;

namespace PaymentGateway.Api.Models.Domain;

public class PaymentError(PaymentErrorType errorType, string message, object? data = null) : Error(message)
{
    public PaymentErrorType ErrorType { get; } = errorType;
    public object? Data { get; } = data;

    public PaymentError(PaymentErrorType errorType, IEnumerable<ValidationFailure> errors)
        : this(errorType, "Validation failed", errors.Select(e => e.ErrorMessage).ToList())
    {
    }
}