using System.Net;

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
            return new ActionResult<PostPaymentResponse>(new PostPaymentResponse(postPaymentRequest, PaymentStatus.Rejected.ToString()));
        }
        var response = await paymentService.PostPaymentAsync(postPaymentRequest);
        return response.IsSuccess
            ? new ActionResult<PostPaymentResponse>(response.PostPaymentResponse)
            : (ActionResult<PostPaymentResponse>)BadRequest(response.ErrorMessage);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetPaymentResponse>> GetPaymentAsync(Guid id)
    {
        if (!ModelState.IsValid) { return BadRequest("Error"); }

        var response = await paymentService.GetPaymentByIdAsync(id);
        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                return new ActionResult<GetPaymentResponse>(response.GetPaymentResponse);
            case HttpStatusCode.NotFound:
                return NotFound(response?.ErrorMessage);
            default:
                return BadRequest(response?.ErrorMessage);
        }
    }
}