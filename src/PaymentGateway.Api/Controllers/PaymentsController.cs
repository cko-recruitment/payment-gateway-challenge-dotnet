using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController : ControllerBase
{
    private readonly ILogger<PaymentsController> _logger;
    private readonly IPaymentService _paymentService;

    public PaymentsController(
        ILogger<PaymentsController> logger,
        IPaymentService paymentService)
    {
        _logger = logger;
        _paymentService = paymentService;
    }

    [HttpPost]
    public async Task<ActionResult<PostPaymentResponse>> PostPaymentAsync(
        PostPaymentRequest? request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Post payment action was called.");

        var result = await _paymentService.ProcessPaymentAsync(request, cancellationToken);

        if (result.Status == PaymentServiceResultStatus.Created)
        {
            return CreatedAtRoute("GetPaymentById", new { id = result.Result!.Id }, result.Result);
        }
        if (result.Status == PaymentServiceResultStatus.Rejected)
        {
            return BadRequest(result.Result);
        }

        if (result.Status == PaymentServiceResultStatus.Unavailable)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        }

        return StatusCode(StatusCodes.Status500InternalServerError);
    }

    [HttpGet("{id:guid}", Name = "GetPaymentById")]
    public ActionResult<GetPaymentResponse> GetPaymentAsync(Guid id)
    {
        _logger.LogInformation("Get payment action was called for payment id {PaymentId}.", id);

        var result = _paymentService.GetPayment(id);

        if (result.Status == PaymentServiceResultStatus.Found)
        {
            return Ok(result.Result);
        }

        if (result.Status == PaymentServiceResultStatus.NotFound)
        {
            return NotFound();
        }

        return StatusCode(StatusCodes.Status500InternalServerError);
    }
}
