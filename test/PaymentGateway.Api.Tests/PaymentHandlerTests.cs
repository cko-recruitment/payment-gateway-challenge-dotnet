using System.Net;
using Moq;
using Moq.Protected;

using PaymentGateway.Api.Interfaces;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Tests
{
    public class PaymentHandlerTests
    {
        private readonly PaymentsHandler sut;
        private readonly Mock<IPaymentsRepository> repositoryMock = new();
        private readonly Mock<IPaymentRequestValidator> paymentValidatorMock = new();
        private readonly Mock<IHttpClientFactory> clientFactoryMock = new();

        public PaymentHandlerTests()
        {
            sut = new PaymentsHandler(repositoryMock.Object, paymentValidatorMock.Object, clientFactoryMock.Object);
        }

        [Fact]
        public async void HandleGetPaymentAsync_InvalidRequest_ReturnsRejectResponse()
        {
            // Arrange
            ValidatedPostPaymentRequest request = new();
            PostPaymentRequest postPaymentRequest = new();
            paymentValidatorMock.Setup(x => x.Validate(It.IsAny<PostPaymentRequest>(), out request)).Returns(false);

            // Act
            var result = await sut.HandlePostPaymentAsync(postPaymentRequest);

            // Act 
            Assert.True(result.Status == PaymentStatus.Rejected);
        }


        [Fact]
        public async void HandleGetPaymentAsync_AuthorizedRequest_ReturnsAuthorizedResponse()
        {
            // Arrange
            string json = $$"""
    {
        "authorized": true,
        "authorization_code":"{{Guid.NewGuid()}}"

    }
    """;
            ValidatedPostPaymentRequest request = new();
            PostPaymentRequest postPaymentRequest = new();
            paymentValidatorMock.Setup(x => x.Validate(It.IsAny<PostPaymentRequest>(), out request)).Returns(true);
            Mock<HttpMessageHandler> messageHandlerMock = new();
            messageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
                )
   .ReturnsAsync(new HttpResponseMessage()
   {
       StatusCode = HttpStatusCode.OK,
       Content = new StringContent(json),
   })
   .Verifiable();
            var client = new HttpClient(messageHandlerMock.Object);

            clientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(client);

            // Act
            var result = await sut.HandlePostPaymentAsync(postPaymentRequest);

            // Act 
            Assert.NotNull(result);
            Assert.True(result.Status == PaymentStatus.Authorized);
        }

        [Fact]
        public async void HandleGetPaymentAsync_DeclinedRequest_ReturnsAuthorizedResponse()
        {
            // Arrange
            string json = $$"""
    {
        "authorized": false,
        "authorization_code":"{{Guid.NewGuid()}}"

    }
    """;

            ValidatedPostPaymentRequest request = new();
            PostPaymentRequest postPaymentRequest = new();
            paymentValidatorMock.Setup(x => x.Validate(It.IsAny<PostPaymentRequest>(), out request)).Returns(true);
            Mock<HttpMessageHandler> messageHandlerMock = new();
            messageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
                )
   .ReturnsAsync(new HttpResponseMessage()
   {
       StatusCode = HttpStatusCode.OK,
       Content = new StringContent(json),
   })
   .Verifiable();
            var client = new HttpClient(messageHandlerMock.Object);

            clientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(client);

            // Act
            var result = await sut.HandlePostPaymentAsync(postPaymentRequest);

            // Act 
            Assert.NotNull(result);
            Assert.True(result.Status == PaymentStatus.Declined);
        }
    }
}
