using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using PaymentGateway.Api.Models;

namespace PaymentGateway.Api.Controllers;

public class PaymentsControllerExceptionFilter(ILogger<PaymentsControllerExceptionFilter> logger) : IAsyncExceptionFilter
{
    public Task OnExceptionAsync(ExceptionContext context)
    {
        if (context.Exception is BankException bankException)
        {
            logger.LogError(
                "Bank request failed with status {BankStatusCode}. Message: {BankMessage}. Trace {TraceId}",
                bankException.StatusCode,
                string.IsNullOrWhiteSpace(bankException.Message)
                    ? "No error message provided."
                    : bankException.Message, // In production, sanitize the message to redact any sensitive information before logging.
                context.HttpContext.TraceIdentifier);

            context.Result = new ObjectResult(new ProblemDetails
            {
                // Return a generic failure while retaining the bank status in internal logs.
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred while processing the payment."
            });
                    
            context.ExceptionHandled = true;
        }
        
        return Task.CompletedTask;
    }
}