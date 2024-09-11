using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Moq;
using Moq.Protected;

using PaymentGateway.Api.Repositories;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Constants.Enums;
using System.Text.Json;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Tests.Repositories
{
    public class PaymentRepositoryTests
    {
        private const string BankUrl = "http://bankurl.com";
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly Mock<ILogger<PaymentRepository>> _mockLogger;
        private readonly PaymentRepository _paymentRepository;
        private readonly PostPaymentRequest _postPaymentRequest;
        private readonly PostToBankResponse _postToBankResponse;
        private readonly PostPaymentRequestDto _postPaymentRequestDto;
        private HttpResponseMessage _httpResponseMessage;
        private const string AuthorizationCode = "551231213123";
        private bool Authorized = true;
        private HttpStatusCode httpStatusCode = HttpStatusCode.OK;


        public PaymentRepositoryTests()
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
            _postPaymentRequestDto = new PostPaymentRequestDto(_postPaymentRequest);

            _postToBankResponse = new PostToBankResponse()
            {
                AuthorizationCode = AuthorizationCode,
                Authorized = Authorized,
            };

            _httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = httpStatusCode,
                Content = new StringContent(JsonSerializer.Serialize(_postToBankResponse))
            };
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _mockHttpMessageHandler.Protected()
                              .Setup<Task<HttpResponseMessage>>("SendAsync",
                                  ItExpr.IsAny<HttpRequestMessage>(),
                                  ItExpr.IsAny<CancellationToken>())
                              .ReturnsAsync(() => _httpResponseMessage);

            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(() =>
                {
                    var client = new HttpClient(_mockHttpMessageHandler.Object)
                    {
                        BaseAddress = new Uri(BankUrl)
                    };
                    return client;
                }); _mockLogger = new Mock<ILogger<PaymentRepository>>();
            _paymentRepository = new PaymentRepository(_mockLogger.Object, _mockHttpClientFactory.Object, BankUrl);
        }

        [Fact]
        public async Task PostAsync_ShouldReturnSuccess_WhenValidRequest()
        {
            // Arrange
            _httpResponseMessage.StatusCode = HttpStatusCode.OK;

            // Act
            var result = await _paymentRepository.PostAsync(_postPaymentRequestDto);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(AuthorizationCode, result.PostToBankResponse.AuthorizationCode);
            Assert.True(result.PostToBankResponse.Authorized);
        }

        [Fact]
        public async Task PostAsync_ShouldReturnFail_WhenNotAuthorized()
        {
            // Arrange
            _postToBankResponse.Authorized = false;
            _postToBankResponse.AuthorizationCode = string.Empty;
            _httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent(JsonSerializer.Serialize(_postToBankResponse))
            };

            // Act
            var result = await _paymentRepository.PostAsync(_postPaymentRequestDto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.False(result.PostToBankResponse.Authorized);
            Assert.Contains("Error while processing payment, bank returned error.", result.ErrorMessage);
        }

        // bank endpoint down or any other error occurs
        [Fact]
        public async Task PostAsync_ShouldReturnFailedResult_WhenUnexpectedError()
        {
            // Arrange
            _httpResponseMessage.Content = null;

            // Act
            var result = await _paymentRepository.PostAsync(_postPaymentRequestDto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Null(result.PostToBankResponse);
            Assert.Contains("An unexpected error occurred", result.ErrorMessage);
        }
    }
}
