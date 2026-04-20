using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using PaymentGateway.Api.Exceptions;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services.Interfaces;
using PaymentGateway.Api.Validators;

namespace PaymentGateway.Api.Controllers;

/// <summary>
/// Payment Gateway API Controller
/// Handles payment processing and retrieval operations.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class PaymentsController : Controller
{
    private readonly IRequestValidator _requestValidator;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IRequestValidator requestValidator,
        IPaymentService paymentService,
        ILogger<PaymentsController> logger)
    {
        _requestValidator = requestValidator;
        _paymentService = paymentService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a payment by its ID.
    /// </summary>
    /// <param name="id">The unique identifier of the payment.</param>
    /// <returns>
    /// 200 OK: Payment found and returned.
    /// 404 Not Found: Payment with the specified ID does not exist.
    /// 500 Internal Server Error: An unexpected error occurred.
    /// </returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaymentResponse>> GetPaymentAsync(Guid id)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object> { { "PaymentId", id } });

        try
        {
            _logger.LogInformation("GET payment request received");
            var payment = _paymentService.Get(id);
            _logger.LogInformation("Payment retrieved successfully");

            return Ok(payment);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Payment not found");
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment");
            return StatusCode(500, ex.Message);
        }
    }

    /// <summary>
    /// Processes a payment through the payment gateway.
    /// </summary>
    /// <param name="request">The payment request containing card and transaction details.</param>
    /// <param name="idempotencyKey">Unique key for idempotency to prevent duplicate payments.</param>
    /// <returns>
    /// 200 OK: Payment processed successfully (Authorized or Declined).
    /// 400 Bad Request: Invalid payment data provided (Rejected).
    /// 409 Conflict: Duplicate payment detected (same Idempotency-Key).
    /// 500 Internal Server Error: An unexpected error occurred.
    /// </returns>
    [HttpPost]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IDictionary<string, string[]>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaymentResponse>> PostPaymentAsync(
        [FromBody] PostPaymentRequest request,
        [FromHeader(Name = "Idempotency-Key")] string idempotencyKey)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object> { { "IdempotencyKey", idempotencyKey } });

        try
        {
            _logger.LogInformation("Payment request received for amount {Amount} {Currency}",
                request.Amount, request.Currency);
            
            _requestValidator.Validate(request, idempotencyKey);
            _logger.LogInformation("Payment request validation passed");

            var paymentResponse = await _paymentService.ProcessPaymentAsync(request, Guid.Parse(idempotencyKey));

            _logger.LogInformation("Payment processed successfully");
            return Ok(paymentResponse);
        }
        catch (DuplicatePaymentException ex)
        {
            _logger.LogWarning("Duplicate payment detected");
            return Conflict(ex.Message);
        }
        catch (PaymentValidationException ex)
        {
            _logger.LogWarning("Payment validation failed");
            return BadRequest(ex.Errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment");
            return StatusCode(500, ex.Message);
        }
    }
}