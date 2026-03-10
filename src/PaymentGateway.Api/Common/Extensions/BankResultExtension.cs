using FluentResults;

using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Models.Domain;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Common.Extensions;

public static class BankResultExtensions
{
    private static readonly string[] ServiceUnavailableTerms = ["Service Unavailable", "Error connecting", "Rejected"];

    public static PaymentError ToPaymentError(this Result<BankSimulatorResponse> bankResult)
    {
        var message = bankResult.Errors.First().Message;
        var errorType = ServiceUnavailableTerms.Any(message.Contains)
            ? PaymentErrorType.ServiceUnavailable
            : PaymentErrorType.ValidationFailed;

        return new PaymentError(errorType, message);
    }
}