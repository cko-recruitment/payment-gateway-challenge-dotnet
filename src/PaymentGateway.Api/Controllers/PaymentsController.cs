using System.Diagnostics;

using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[TypeFilter(typeof(PaymentsControllerExceptionFilter))]

public class PaymentsController(
    PaymentsRepository paymentsRepository,
    IAcquiringBankClient acquiringBankClient,
    TimeProvider timeProvider)
    : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<PaymentResponse>> CreatePaymentAsync(
        [FromBody] CreatePaymentRequest request,
        CancellationToken cancellationToken)
    {
        request.RecordInActivityTags(); // this can become a generic middleware as a cross-cutting concern

        if (PaymentValidator.Validate(request, timeProvider.GetUtcNow().DateTime) is { } errors)
        {
            return BadRequest(errors.ToProblemDetails());
        }

        var bankAuthResult = await acquiringBankClient.AuthorizeAsync(request.ToBankPaymentRequest(), cancellationToken);
        var bankAccountAuth = bankAuthResult is null
            ? new BankAuthorization(PaymentStatus.Declined)
            : new BankAuthorization(PaymentStatus.Authorized, bankAuthResult);

        var payment = request.ToPaymentModel(bankAccountAuth);
        paymentsRepository.Add(payment);

        Activity.Current?.SetTag("payment.id", payment.Id);

        var response = payment.ToResponse();
        return CreatedAtRoute(nameof(GetPaymentAsync), new { id = response.Id }, response);
    }

    [HttpGet("{id:guid}", Name = nameof(GetPaymentAsync))]
    public async Task<ActionResult<PaymentResponse?>> GetPaymentAsync(Guid id)
    {
        var payment = paymentsRepository.Get(id);

        return payment?.ToResponse() switch
        {
            { } response => Ok(response),
            _ => Problem(statusCode: StatusCodes.Status404NotFound, title: "Payment not found.")
        };
    }
}