using System.Net;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;

using Moq;

using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Tests.Controllers;

public class PaymentControllerTests
{
    private readonly PaymentController _paymentController;
    private readonly Mock<IPaymentService> _mockPaymentService;
    private readonly PostPaymentResult _postPaymentResult;
    private readonly PostPaymentRequest _postPaymentRequest;
    private readonly PostToBankResponse _postToBankResponse;
    private readonly PostPaymentResponse _postPaymentResponse;

    public PaymentControllerTests()
    {
        _postPaymentRequest = new PostPaymentRequest()
        {
            CardNumber = "1234567890123456",
            ExpiryMonth = 12,
            ExpiryYear = 2028,
            Amount = 100,
            Currency = "GBP",
            Cvv = "123"
        };

        _postToBankResponse = new PostToBankResponse()
        {
            AuthorizationCode = "1231231",
            Authorized = true
        };

        _postPaymentResponse = new PostPaymentResponse(_postPaymentRequest, _postToBankResponse);
        _postPaymentResult = new PostPaymentResult(true, _postPaymentResponse);
        _mockPaymentService = new Mock<IPaymentService>();
        _mockPaymentService.Setup(p => p.ProcessPaymentAsync(It.IsAny<PostPaymentRequest>()))
            .ReturnsAsync(() => _postPaymentResult);
        _paymentController = new PaymentController(_mockPaymentService.Object);
    }

    [Fact]
    public async Task PostPaymentAsync_Should_Generate_Successful_Response()
    {
        // Arrange
        var postPaymentRequest = new PostPaymentRequest()
        {
            CardNumber = "1234567890123456",
            ExpiryMonth = 12,
            ExpiryYear = 2028,
            Amount = 100,
            Currency = "GBP",
            Cvv = "123"
        };

        // Act
        var result = await _paymentController.PostPaymentAsync(postPaymentRequest);

        // Assert
        Assert.Equal(_postPaymentResponse.Status, result.Value.Status);
        _mockPaymentService.Verify(x => x.ProcessPaymentAsync(postPaymentRequest), Times.Once());
    }

    [Fact]
    public async Task PostPaymentAsync_Should_ReturnBadRequest_WhenFails()
    {
        // Arrange
        _mockPaymentService.Setup(p => p.ProcessPaymentAsync(It.IsAny<PostPaymentRequest>()))
            .ReturnsAsync(() => new PostPaymentResult(false, new PostPaymentResponse(_postPaymentRequest, _postToBankResponse), "Error"));

        // Act
        var result = await _paymentController.PostPaymentAsync(new PostPaymentRequest());

        // Assert
        Assert.Equal((int)HttpStatusCode.BadRequest, ((IStatusCodeActionResult)result.Result).StatusCode);
        Assert.Equal("Error", ((BadRequestObjectResult)result.Result).Value);
    }
}