using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Moq;

using PaymentGateway.Api.Constants.Enums;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Repositories;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Tests.Services
{
    public class PaymentServiceTests
    {
        private readonly PaymentService _paymentService;
        private readonly Mock<IPaymentRepository> _paymentRepository;
        private readonly Mock<ILogger<PaymentService>> _logger;
        private readonly PostPaymentRequestDto _postPaymentRequestDto;
        private readonly PostToBankResult _postToBankResult;
        private readonly PostPaymentRequest _postPaymentRequest;
        private readonly PostToBankResponse _postToBankResponse;
        private const string AuthorizationCode = "1231231";
        private const bool Authorized = true;

        public PaymentServiceTests()
        {
            _postPaymentRequest = new()
            {
                Amount = 1000,
                CardNumber = "123124144231",
                Currency = Currencies.GBP.ToString(),
                Cvv = "123",
                ExpiryMonth = 5,
                ExpiryYear = DateTime.Now.Year + 100
            };

            _postToBankResponse = new PostToBankResponse()
            {
                AuthorizationCode = AuthorizationCode,
                Authorized = Authorized
            };

            _postToBankResult = new PostToBankResult(true, _postToBankResponse);
            _postPaymentRequestDto = new PostPaymentRequestDto(_postPaymentRequest);
            _logger = new Mock<ILogger<PaymentService>>();

            _paymentRepository = new Mock<IPaymentRepository>();
            _paymentRepository.Setup(x => x.PostAsync(It.IsAny<PostPaymentRequestDto>()))
                .ReturnsAsync(() => _postToBankResult);
            _paymentService = new PaymentService(_logger.Object, _paymentRepository.Object);
        }

        [Fact]
        public async Task ProcessPaymentAsync_ReturnsSuccess()
        {
            // Arrange
            var expectedCardNumberLastFour = int.Parse(_postPaymentRequest.CardNumber[^4..]);
            var expectedAmount = _postPaymentRequest.Amount;

            // Act
            var result = await _paymentService.PostPaymentAsync(_postPaymentRequest);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(expectedCardNumberLastFour, result.PostPaymentResponse.CardNumberLastFour);
            Assert.Equal(expectedAmount, result.PostPaymentResponse.Amount);
        }

        [Fact]
        public async Task ProcessPaymentAsync_Should_ReturnFailure_WhenInvalidExpiryDate()
        {
            // Arrange
            _postPaymentRequest.ExpiryMonth = 1;
            _postPaymentRequest.ExpiryYear = DateTime.Now.Year - 2;
            var expectedErrorMessage = $"Expiry date of {_postPaymentRequest.ExpiryMonth}/{_postPaymentRequest.ExpiryYear} is not valid";

            // Act
            var result = await _paymentService.PostPaymentAsync(_postPaymentRequest);

            // Assert
            Assert.Contains(expectedErrorMessage, result.ErrorMessage);
            Assert.False(result.IsSuccess);
            Assert.True(result.PostPaymentResponse.Status == PaymentStatus.Rejected.ToString());
        }

        [Fact]
        public async Task ProcessPaymentAsync_Should_ReturnFailure_WhenPaymentNotAuthorized()
        {
            // Arrange
            _postToBankResponse.Authorized = false;
            _postToBankResponse.AuthorizationCode = string.Empty;

            var postToBankResult = new PostToBankResult(false, _postToBankResponse);
            _paymentRepository.Setup(x => x.PostAsync(It.IsAny<PostPaymentRequestDto>()))
                .ReturnsAsync(() => postToBankResult);

            // Act
            var result = await _paymentService.PostPaymentAsync(_postPaymentRequest);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.PostPaymentResponse.Status == PaymentStatus.Declined.ToString());
        }

        


    }
}
