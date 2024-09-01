using System.Net;

using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Interfaces;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController(IPaymentsHandler paymentsHandler) : Controller
{
    private readonly IPaymentsHandler _paymentsHandler = paymentsHandler;

    /// <summary>
    /// Controller method for getting a payment
    /// </summary>
    /// <param name="id">The <see cref="Guid"/> Id of the payment</param>
    /// <returns></returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PostPaymentResponse?>> GetPaymentAsync(Guid id)
    {
        var payment = _paymentsHandler.HandleGetPaymentAsync(id);

        return payment is null ? new NotFoundResult() : new OkObjectResult(payment);
    }

    /// <summary>
    /// Controller method for posting a new payment
    /// </summary>
    /// <param name="paymentRequest">the <see cref="PostPaymentRequest"/> object</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<PostPaymentResponse>> PostPaymentAsync([FromBody] PostPaymentRequest paymentRequest)
    {

        var paymentResponse = await _paymentsHandler.HandlePostPaymentAsync(paymentRequest);

        return paymentResponse.Status == PaymentStatus.Authorized 
            ? new OkObjectResult(paymentResponse) 
            : new BadRequestObjectResult($"Error submitting payment with Id {paymentResponse.Id}");
    }
}