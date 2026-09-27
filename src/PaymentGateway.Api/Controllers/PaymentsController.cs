using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController : Controller
{
    private readonly PaymentsRepository _paymentsRepository;
    private readonly TimeProvider _timeProvider;
    private readonly IAcquiringBankClient _acquiringBankClient;

    public PaymentsController(
        PaymentsRepository paymentsRepository, 
        IAcquiringBankClient acquiringBankClient,
        TimeProvider timeProvider)
    {
        _paymentsRepository = paymentsRepository;
        _acquiringBankClient = acquiringBankClient;
        _timeProvider = timeProvider;
    }

    [HttpPost]
    public async Task<ActionResult<PaymentResponse>> CreatePaymentAsync(
        [FromBody] CreatePaymentRequest request,
        CancellationToken cancellationToken)
    {
        if (PaymentValidator.Validate(request, _timeProvider.GetUtcNow().DateTime) is { } errors)
        {
            return BadRequest(new ValidationProblemDetails
            {
                Title = "Payment request is invalid.",
                Status = StatusCodes.Status400BadRequest,
                Detail = $"Field '{errors.Field}' is invalid: {string.Join(", ", errors.Messages)}"
            });
        }
        
        var bankAuthResult = await _acquiringBankClient.AuthorizeAsync(request.ToPayment(), cancellationToken);

        var bankAuthStatus = bankAuthResult is null ? PaymentStatus.Declined : PaymentStatus.Authorized;
        
        var bankAccountAuth = new BankAuthorization(bankAuthStatus, bankAuthResult);

        var payment = request.ToPayment(bankAccountAuth);
        
        _paymentsRepository.Add(payment);

        var  response = payment.ToResponse();
        
        return CreatedAtAction(nameof(GetPaymentAsync), new { id = response.Id }, response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PaymentResponse?>> GetPaymentAsync(Guid id)
    {
        var payment = _paymentsRepository.Get(id);
        
        return payment?.ToResponse() switch
        {
            { } response => Ok(response),
            _ => Problem(statusCode: StatusCodes.Status404NotFound, title: "Payment not found.")
        };
    }
}