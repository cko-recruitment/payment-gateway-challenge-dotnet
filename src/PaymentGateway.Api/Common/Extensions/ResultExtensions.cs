using FluentResults;

using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Models.Domain;

namespace PaymentGateway.Api.Common.Extensions;

public static class ResultExtensions
{
    public static ActionResult<T> ToActionResult<T>(this Result<T> result, ControllerBase controller)
    {
        if (result.IsSuccess) return controller.Ok(result.Value);

        var paymentError = result.Errors.OfType<PaymentError>().FirstOrDefault();

        if (paymentError != null)
        {
            return paymentError.ErrorType switch
            {
                PaymentErrorType.ServiceUnavailable => controller.StatusCode(503,
                    paymentError.Data ?? result.Errors.Select(e => e.Message)),
                PaymentErrorType.NotFound => controller.NotFound(paymentError.Message),
                PaymentErrorType.Conflict => controller.Conflict(paymentError.Message),
                PaymentErrorType.ValidationFailed => controller.BadRequest(paymentError.Data ?? result.Errors.Select(e => e.Message)),
                _ => controller.StatusCode(500, "Unexpected error")
            };
        }

        return controller.BadRequest(result.Errors.Select(e => e.Message));
    }
}