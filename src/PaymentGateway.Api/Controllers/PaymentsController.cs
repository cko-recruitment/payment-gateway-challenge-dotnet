using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController : Controller
{
    private readonly PaymentsRepository _paymentsRepository;

    private readonly IPaymentProcessor _processor;

    public PaymentsController(PaymentsRepository paymentsRepository, IPaymentProcessor processor)
    {
        _paymentsRepository = paymentsRepository;
        _processor = processor;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PostPaymentResponse?>> GetPaymentAsync(Guid id)
    {
        var payment = _paymentsRepository.Get(id);

        if (payment == null)
        {
            return NotFound();
        }
        
        var response = new GetPaymentResponse
        {
            Id = payment.Id,
            Status = payment.Status,
            CardNumberLastFour = payment.CardNumberLastFour,
            ExpiryMonth = payment.ExpiryMonth,
            ExpiryYear = payment.ExpiryYear,
            Currency = payment.Currency,
            Amount = payment.Amount
        };

        return Ok(response);
    }
    
    [HttpPost]
    public async Task<ActionResult<PostPaymentResponse>> CreatePayment([FromBody] PostPaymentRequest request)
    {
        var result = await _processor.ProcessAsync(request);

        if (result.Status == PaymentStatus.Rejected)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }
}