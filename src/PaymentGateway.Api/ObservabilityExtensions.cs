using System.Diagnostics;

using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api;

public static class ObservabilityExtensions
{
    public static void RecordInActivityTags(this CreatePaymentRequest request)
    {
        Activity.Current?
            .SetTag("payment.amount", request.Amount)
            .SetTag("payment.currency", request.Currency)
            .SetTag("payment.cardNumber", $"*{request.CardNumber?.Length}*")
            .SetTag("payment.cvv", $"*{request.Cvv?.Length}*")
            .SetTag("payment.expiryMonth", request.ExpiryMonth)
            .SetTag("payment.expiryYear", request.ExpiryYear);
    }
}