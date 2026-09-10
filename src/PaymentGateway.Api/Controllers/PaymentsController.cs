using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentsService _paymentsService;

    public PaymentsController(IPaymentsService paymentsService)
    {
        _paymentsService = paymentsService;
    }

    [HttpPost]
    public async Task<ActionResult<PostPaymentResponse>> ProcessPaymentAsync(PostPaymentRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Ok(new PostPaymentResponse
            {
                Id = Guid.NewGuid(),
                Status = PaymentStatus.Rejected,
                CardNumberLastFour = request.CardNumber.Length >= 4 ? request.CardNumber[^4..] : request.CardNumber,
                ExpiryMonth = request.ExpiryMonth,
                ExpiryYear = request.ExpiryYear,
                Currency = request.Currency,
                Amount = request.Amount
            });
        }

        var result = await _paymentsService.ProcessPaymentAsync(request, cancellationToken);
        if (result.BankUnavailable)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                message = "The acquiring bank is currently unavailable. Please try again later."
            });
        }

        return Ok(result.Response);
    }

    [HttpGet("{id:guid}")]
    public ActionResult<GetPaymentResponse> GetPaymentAsync(Guid id)
    {
        var payment = _paymentsService.GetPayment(id);

        return payment is null ? NotFound() : Ok(payment);
    }
}
