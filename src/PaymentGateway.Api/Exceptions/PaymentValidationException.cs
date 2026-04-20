namespace PaymentGateway.Api.Exceptions;

public class PaymentValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public PaymentValidationException(IDictionary<string, string[]> errors) : base("Payment validation failed")
    {
        Errors = errors;
    }
}
