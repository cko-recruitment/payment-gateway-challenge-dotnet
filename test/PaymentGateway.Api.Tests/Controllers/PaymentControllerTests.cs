using System.Net;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

using Moq;

using PaymentGateway.Api.Constants.Enums;
using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Models.Results;
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
    private readonly GetPaymentResponse _getPaymentResponse;
    private readonly GetPaymentResult _getPaymentResult;

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

        _getPaymentResponse = new()
        {
            CardNumber = "1234567890123456",
            ExpiryMonth = 12,
            ExpiryYear = 2028,
            Amount = 100,
            Currency = "GBP",
            Status = PaymentStatus.Authorized,
            Id = Guid.NewGuid()
        };

        _getPaymentResult = new GetPaymentResult()
        {
            IsSuccess = true,
            GetPaymentResponse = _getPaymentResponse,
            StatusCode = HttpStatusCode.OK
        };

        _postPaymentResponse = new PostPaymentResponse(_postPaymentRequest, _postToBankResponse.Authorized.ToString());
        _postPaymentResult = new PostPaymentResult(true, _postPaymentResponse);
        _mockPaymentService = new Mock<IPaymentService>();
        _mockPaymentService.Setup(p => p.PostPaymentAsync(It.IsAny<PostPaymentRequest>()))
            .ReturnsAsync(() => _postPaymentResult);
        _mockPaymentService.Setup(p => p.GetPaymentByIdAsync(It.IsAny<Guid>())).ReturnsAsync(() => _getPaymentResult);
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
        _mockPaymentService.Verify(x => x.PostPaymentAsync(postPaymentRequest), Times.Once());
    }

    [Fact]
    public async Task PostPaymentAsync_Should_ReturnBadRequest_WhenFails()
    {
        // Arrange
        _mockPaymentService.Setup(p => p.PostPaymentAsync(It.IsAny<PostPaymentRequest>()))
            .ReturnsAsync(() => new PostPaymentResult(false, new PostPaymentResponse(_postPaymentRequest, _postToBankResponse.Authorized.ToString()), "Error"));

        // Act
        var result = await _paymentController.PostPaymentAsync(new PostPaymentRequest());

        // Assert
        Assert.Equal((int)HttpStatusCode.BadRequest, ((IStatusCodeActionResult)result.Result).StatusCode);
        Assert.Equal("Error", ((BadRequestObjectResult)result.Result).Value);
    }

    [Fact]
    public async Task GetPaymentAsync_Should_Generate_Successful_Response()
    {
        // Arrange
        var id = Guid.NewGuid();
        _getPaymentResponse.Id = id;

        // Act
        var result = await _paymentController.GetPaymentAsync(id);

        // Assert
        Assert.Equal(_getPaymentResponse.Id, result.Value.Id);
        _mockPaymentService.Verify(x => x.GetPaymentByIdAsync(id), Times.Once());
    }

    [Fact]
    public async Task GetPaymentAsync_Should_ReturnNotFound_WhenIdNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockPaymentService.Setup(p => p.GetPaymentByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(() => new GetPaymentResult()
            {
                IsSuccess = false,
                StatusCode = HttpStatusCode.NotFound,
            });

        // Act
        var result = await _paymentController.GetPaymentAsync(id);

        // Assert
        Assert.Equal((int)HttpStatusCode.NotFound, ((IStatusCodeActionResult)result.Result).StatusCode);
    }

    [Fact]
    public async Task GetPaymentAsync_Should_ReturnBadRequest_WhenServiceFails()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockPaymentService.Setup(p => p.GetPaymentByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(() => new GetPaymentResult()
            {
                IsSuccess = false,
                StatusCode = HttpStatusCode.InternalServerError,
            });

        // Act
        var result = await _paymentController.GetPaymentAsync(id);

        // Assert
        Assert.Equal((int)HttpStatusCode.BadRequest, ((IStatusCodeActionResult)result.Result).StatusCode);
    }
}