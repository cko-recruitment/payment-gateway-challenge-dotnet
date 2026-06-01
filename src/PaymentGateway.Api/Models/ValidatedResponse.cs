using PaymentGateway.Api.Enums;

namespace PaymentGateway.Api.Models;

public class ValidatedResponse<T>
{
    public PaymentServiceResultStatus Status { get; set; }
    public T? Result { get; set; }
}
