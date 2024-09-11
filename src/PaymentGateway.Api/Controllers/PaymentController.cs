using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Constants.Enums;
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
        if (!ModelState.IsValid)
        {
            return new ActionResult<PostPaymentResponse>(new PostPaymentResponse(postPaymentRequest)
            {
                Status = PaymentStatus.Rejected.ToString()
            });
        }
        var response = await paymentService.ProcessPaymentAsync(postPaymentRequest);
        return response.IsSuccess
            ? new ActionResult<PostPaymentResponse>(response.PostPaymentResponse)
            : (ActionResult<PostPaymentResponse>)BadRequest(response.ErrorMessage);
    }
}