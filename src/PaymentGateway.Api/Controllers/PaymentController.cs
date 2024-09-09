using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentController(IPaymentService paymentService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<PostPaymentResponse>> PostPaymentAsync([FromBody] PostPaymentRequest postPaymentRequest)
    {
        var response = await paymentService.ProcessPaymentAsync(postPaymentRequest);
        return response.IsSuccess
            ? new ActionResult<PostPaymentResponse>(response.PostPaymentResponse)
            : (ActionResult<PostPaymentResponse>)BadRequest(response.ErrorMessage);
    }
}