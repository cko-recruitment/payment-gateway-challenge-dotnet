using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Controllers;

public static class ValidationErrorExtensions
{
    extension(ValidationError error)
    {
        public ValidationProblemDetails ToProblemDetails() =>
            new()
            {
                Title = "Request is invalid.",
                Status = StatusCodes.Status400BadRequest,
                Detail = $"Field '{error.Field}' is invalid: {string.Join(", ", error.Messages)}"
            };
    }
}