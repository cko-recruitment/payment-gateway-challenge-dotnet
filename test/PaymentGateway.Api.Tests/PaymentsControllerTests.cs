using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Models.Bank;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Tests;

public class PaymentsControllerTests
{
    private readonly Random _random = new();

    private static WebApplicationFactory<PaymentsController> CreateFactory(
        IPaymentsRepository? paymentsRepository = null,
        IBankClient? bankClient = null)
    {
        var factory = new WebApplicationFactory<PaymentsController>();
        return factory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services =>
            {
                if (paymentsRepository is not null)
                {
                    services.AddSingleton(paymentsRepository);
                }

                if (bankClient is not null)
                {
                    services.AddSingleton(bankClient);
                }
            }));
    }

    private static PostPaymentRequest CreateValidRequest(string cardNumber = "2222405343248112") => new()
    {
        CardNumber = cardNumber,
        ExpiryMonth = 4,
        ExpiryYear = DateTime.UtcNow.Year + 1,
        Currency = "GBP",
        Amount = 100,
        Cvv = "123"
    };

    [Fact]
    public async Task RetrievesAPaymentSuccessfully()
    {
        // Arrange
        var payment = new PostPaymentResponse
        {
            Id = Guid.NewGuid(),
            Status = PaymentStatus.Authorized,
            ExpiryYear = _random.Next(2030, 2040),
            ExpiryMonth = _random.Next(1, 12),
            Amount = _random.Next(1, 10000),
            CardNumberLastFour = "1234",
            Currency = "GBP"
        };

        var paymentsRepository = new PaymentsRepository();
        paymentsRepository.Add(payment);

        var client = CreateFactory(paymentsRepository: paymentsRepository).CreateClient();

        // Act
        var response = await client.GetAsync($"/api/Payments/{payment.Id}");
        var paymentResponse = await response.Content.ReadFromJsonAsync<GetPaymentResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(paymentResponse);
        Assert.Equal(payment.Id, paymentResponse!.Id);
        Assert.Equal(payment.CardNumberLastFour, paymentResponse.CardNumberLastFour);
    }

    [Fact]
    public async Task Returns404IfPaymentNotFound()
    {
        // Arrange
        var client = CreateFactory().CreateClient();

        // Act
        var response = await client.GetAsync($"/api/Payments/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ProcessPayment_ReturnsAuthorized_WhenBankAuthorizesThePayment()
    {
        // Arrange
        var bankClientMock = new Mock<IBankClient>();
        bankClientMock
            .Setup(x => x.ProcessPaymentAsync(It.IsAny<BankPaymentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BankPaymentResult.Success(true, Guid.NewGuid().ToString()));

        var client = CreateFactory(bankClient: bankClientMock.Object).CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/Payments", CreateValidRequest(cardNumber: "2222405343248113"));
        var paymentResponse = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(paymentResponse);
        Assert.Equal(PaymentStatus.Authorized, paymentResponse!.Status);
        Assert.Equal("8113", paymentResponse.CardNumberLastFour);
    }

    [Fact]
    public async Task ProcessPayment_ReturnsDeclined_WhenBankDeclinesThePayment()
    {
        // Arrange
        var bankClientMock = new Mock<IBankClient>();
        bankClientMock
            .Setup(x => x.ProcessPaymentAsync(It.IsAny<BankPaymentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BankPaymentResult.Success(false, null));

        var client = CreateFactory(bankClient: bankClientMock.Object).CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/Payments", CreateValidRequest(cardNumber: "2222405343248112"));
        var paymentResponse = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(paymentResponse);
        Assert.Equal(PaymentStatus.Declined, paymentResponse!.Status);
    }

    [Fact]
    public async Task ProcessPayment_Returns503_WhenBankIsUnavailable()
    {
        // Arrange
        var bankClientMock = new Mock<IBankClient>();
        bankClientMock
            .Setup(x => x.ProcessPaymentAsync(It.IsAny<BankPaymentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BankPaymentResult.Unavailable());

        var client = CreateFactory(bankClient: bankClientMock.Object).CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/Payments", CreateValidRequest(cardNumber: "2222405343248110"));

        // Assert
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Theory]
    [InlineData("123")] // too short
    [InlineData("123abc4567890123")] // non-numeric
    public async Task ProcessPayment_ReturnsRejected_WhenCardNumberIsInvalid(string cardNumber)
    {
        // Arrange
        var client = CreateFactory(bankClient: Mock.Of<IBankClient>()).CreateClient();
        var request = CreateValidRequest(cardNumber: cardNumber);

        // Act
        var response = await client.PostAsJsonAsync("/api/Payments", request);
        var paymentResponse = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(paymentResponse);
        Assert.Equal(PaymentStatus.Rejected, paymentResponse!.Status);
    }

    [Fact]
    public async Task ProcessPayment_ReturnsRejected_WhenCurrencyIsNotSupported()
    {
        // Arrange
        var client = CreateFactory(bankClient: Mock.Of<IBankClient>()).CreateClient();
        var request = CreateValidRequest();
        request.Currency = "JPY";

        // Act
        var response = await client.PostAsJsonAsync("/api/Payments", request);
        var paymentResponse = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(paymentResponse);
        Assert.Equal(PaymentStatus.Rejected, paymentResponse!.Status);
    }

    [Fact]
    public async Task ProcessPayment_ReturnsRejected_WhenExpiryDateIsInThePast()
    {
        // Arrange
        var client = CreateFactory(bankClient: Mock.Of<IBankClient>()).CreateClient();
        var request = CreateValidRequest();
        request.ExpiryMonth = 1;
        request.ExpiryYear = DateTime.UtcNow.Year - 1;

        // Act
        var response = await client.PostAsJsonAsync("/api/Payments", request);
        var paymentResponse = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(paymentResponse);
        Assert.Equal(PaymentStatus.Rejected, paymentResponse!.Status);
    }
}
