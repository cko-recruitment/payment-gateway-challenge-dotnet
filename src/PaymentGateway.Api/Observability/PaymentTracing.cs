using System.Diagnostics;
using System.Net.NetworkInformation;

using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Observability;

public static class PaymentTracing
{
    public static void RecordTracing(this CreatePaymentRequest request)
    {
        Activity.Current?
            .SetTag("payment.amount", request.Amount)
            .SetTag("payment.currency", request.Currency)
            .SetTag("payment.cardNumber", $"*{request.CardNumber?.Length}*")
            .SetTag("payment.cvv", $"*{request.Cvv?.Length}*")
            .SetTag("payment.expiryMonth", request.ExpiryMonth)
            .SetTag("payment.expiryYear", request.ExpiryYear);
    }

    public static void RecordTracing(this PaymentModel paymentModel)
    {
        Activity.Current?
            .SetTag("payment.id", paymentModel.Id)
            .SetTag("payment.outcome", paymentModel.BankAuthorization.Status.ToString().ToLowerInvariant());
    }
}