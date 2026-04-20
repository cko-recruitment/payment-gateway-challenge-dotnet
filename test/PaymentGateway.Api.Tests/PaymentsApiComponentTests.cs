using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;
using PaymentGateway.Api.Services.Interfaces;
using PaymentGateway.Api.Tests.Fakers;

namespace PaymentGateway.Api.Tests;

public class PaymentsApiComponentTests
{
    private readonly Random _random = new();
    private readonly PostPaymentRequestFaker _paymentRequestFaker = new();

    private WebApplicationFactory<PaymentsController> CreateWebApplicationFactory(
        IPaymentsRepository repository = null,
        HttpMessageHandler httpMessageHandler = null)
    {
        var factory = new WebApplicationFactory<PaymentsController>();
        return factory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services =>
            {
                if (repository != null)
                    services.AddSingleton(repository);

                var mockHttpHandler = httpMessageHandler ?? new MockHttpMessageHandler();

                if (httpMessageHandler != null)
                {
                    services.AddHttpClient("bank")
                        .ConfigureHttpClient(client =>
                        {
                            client.BaseAddress = new Uri("http://localhost:8080");
                            client.Timeout = TimeSpan.FromSeconds(30);
                        })
                        .ConfigurePrimaryHttpMessageHandler(() => mockHttpHandler);
                }
            }));
    }

    [Fact]
    public async Task GetPayment_ReturnsOk_WhenPaymentExists()
    {
        // Arrange
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            ExpiryYear = _random.Next(2025, 2030),
            ExpiryMonth = _random.Next(1, 12),
            Amount = _random.Next(1, 10000),
            CardNumberLastFour = _random.Next(1111, 9999),
            Currency = "GBP",
            Status = PaymentStatus.Authorized,
            AutorizationCode = "auth-code-123"
        };

        var paymentsRepository = Substitute.For<IPaymentsRepository>();
        paymentsRepository.Get(payment.Id).Returns(payment);

        var webApplicationFactory = CreateWebApplicationFactory(repository: paymentsRepository);
        var client = webApplicationFactory.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/payments/{payment.Id}");
        var paymentResponse = await response.Content.ReadFromJsonAsync<PaymentResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        paymentResponse.Should().NotBeNull();
        paymentResponse!.Id.Should().Be(payment.Id);
        paymentResponse.Currency.Should().Be("GBP");
        paymentResponse.Status.Should().Be(PaymentStatus.Authorized);
        paymentResponse.CardNumberLastFour.Should().Be(payment.CardNumberLastFour);
        
        paymentsRepository.Received(1).Get(payment.Id);
    }

    [Fact]
    public async Task GetPayment_Returns404_WhenPaymentNotFound()
    {
        // Arrange
        var paymentsRepository = Substitute.For<IPaymentsRepository>();
        paymentsRepository.Get(Arg.Any<Guid>()).Returns((Payment)null!);

        var webApplicationFactory = CreateWebApplicationFactory(repository: paymentsRepository);
        var client = webApplicationFactory.CreateClient();
        var paymentId = Guid.NewGuid();

        // Act
        var response = await client.GetAsync($"/api/payments/{paymentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        paymentsRepository.Received(1).Get(paymentId);
    }

    [Fact]
    public async Task PostPayment_Returns200Ok_WhenPaymentIsAuthorized()
    {
        // Arrange
        var paymentsRepository = Substitute.For<IPaymentsRepository>();

        var mockHttpHandler = new MockHttpMessageHandler(authorized: true);

        var webApplicationFactory = CreateWebApplicationFactory(
            repository: paymentsRepository,
            httpMessageHandler: mockHttpHandler);

        var client = webApplicationFactory.CreateClient();
        var idempotencyKey = Guid.NewGuid();

        var paymentRequest = _paymentRequestFaker.Generate();

        // Act
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/payments")
        {
            Content = JsonContent.Create(paymentRequest)
        };
        requestMessage.Headers.Add("Idempotency-Key", idempotencyKey.ToString());

        var response = await client.SendAsync(requestMessage);
        var result = await response.Content.ReadFromJsonAsync<PaymentResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Status.Should().Be(PaymentStatus.Authorized);
        result.Id.Should().Be(idempotencyKey);
        result.Amount.Should().Be(paymentRequest.Amount);
        result.Currency.Should().Be(paymentRequest.Currency);
    }

    [Fact]
    public async Task PostPayment_Returns200Ok_WhenPaymentIsDeclined()
    {
        // Arrange
        var paymentsRepository = Substitute.For<IPaymentsRepository>();

        var mockHttpHandler = new MockHttpMessageHandler(authorized: false);

        var webApplicationFactory = CreateWebApplicationFactory(
            repository: paymentsRepository,
            httpMessageHandler: mockHttpHandler);

        var client = webApplicationFactory.CreateClient();
        var idempotencyKey = Guid.NewGuid();

        var paymentRequest = _paymentRequestFaker.Generate();

        // Act
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/payments")
        {
            Content = JsonContent.Create(paymentRequest)
        };
        requestMessage.Headers.Add("Idempotency-Key", idempotencyKey.ToString());

        var response = await client.SendAsync(requestMessage);
        var result = await response.Content.ReadFromJsonAsync<PaymentResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Status.Should().Be(PaymentStatus.Declined);
        result.Amount.Should().Be(paymentRequest.Amount);
    }

    [Fact]
    public async Task PostPayment_Returns400BadRequest_WhenValidationFails()
    {
        // Arrange
        var paymentsRepository = Substitute.For<IPaymentsRepository>();
        var webApplicationFactory = CreateWebApplicationFactory(repository: paymentsRepository);
        var client = webApplicationFactory.CreateClient();
        var idempotencyKey = Guid.NewGuid().ToString();

        var paymentRequest = _paymentRequestFaker
            .WithCardNumber("123")// Invalid: too short
            .WithMonth(13)        // Invalid: > 12
            .WithCurrency("JPY")  // Invalid: not in allowed list
            .WithCvv("12")        // Invalid: too short
            .Generate();

        // Act
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/payments")
        {
            Content = JsonContent.Create(paymentRequest)
        };
        requestMessage.Headers.Add("Idempotency-Key", idempotencyKey);

        var response = await client.SendAsync(requestMessage);
        var errorResponse = await response.Content.ReadFromJsonAsync<Dictionary<string, string[]>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorResponse.Should().NotBeNull();
        errorResponse!.Should()
            .NotBeEmpty()
            .And.HaveCountGreaterThanOrEqualTo(4)
            .And.ContainKeys("CardNumber", "ExpiryMonth", "Currency", "Cvv");
        
        errorResponse["CardNumber"].Should().Contain(x => x.Contains("14"));
        errorResponse["ExpiryMonth"].Should().Contain(x => x.Contains("12"));
        errorResponse["Currency"].Should().Contain(x => x.Contains("GBP"));
        errorResponse["Cvv"].Should().Contain(x => x.Contains("3"));
    }

    [Fact]
    public async Task PostPayment_Returns409Conflict_WhenIdempotencyKeyDuplicate()
    {
        // Arrange
        var paymentsRepository = Substitute.For<IPaymentsRepository>();
        paymentsRepository.Exists(Arg.Any<Guid>()).Returns(true);

        var webApplicationFactory = CreateWebApplicationFactory(
            repository: paymentsRepository);

        var client = webApplicationFactory.CreateClient();
        var idempotencyKey = Guid.NewGuid();

        var paymentRequest = _paymentRequestFaker.Generate();

        // Act
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/payments")
        {
            Content = JsonContent.Create(paymentRequest)
        };
        requestMessage.Headers.Add("Idempotency-Key", idempotencyKey.ToString());

        var response = await client.SendAsync(requestMessage);
        var responseContent = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        responseContent.Should().Contain("already exists");
    }

    [Fact]
    public async Task PostPayment_Returns400BadRequest_WhenIdempotencyKeyIsInvalid()
    {
        // Arrange
        var paymentsRepository = Substitute.For<IPaymentsRepository>();
        var webApplicationFactory = CreateWebApplicationFactory(repository: paymentsRepository);
        var client = webApplicationFactory.CreateClient();

        var paymentRequest = _paymentRequestFaker.Generate();

        // Act
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/payments")
        {
            Content = JsonContent.Create(paymentRequest)
        };
        requestMessage.Headers.Add("Idempotency-Key", "not-a-guid");

        var response = await client.SendAsync(requestMessage);
        var errorResponse = await response.Content.ReadFromJsonAsync<Dictionary<string, string[]>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        errorResponse.Should().NotBeNull();
        errorResponse!.Should().ContainKey("Idempotency-Key");
        errorResponse["Idempotency-Key"].Should().Contain(x => x.Contains("Guid"));
    }

    [Fact]
    public async Task PostPayment_Returns500_WhenCallToBankApiFails()
    {
        // Arrange
        var mockHttpHandler = new MockHttpMessageHandler(httpStatusCode: HttpStatusCode.ServiceUnavailable);

        var webApplicationFactory = CreateWebApplicationFactory(
            httpMessageHandler: mockHttpHandler);

        var client = webApplicationFactory.CreateClient();
        var idempotencyKey = Guid.NewGuid();

        var paymentRequest = _paymentRequestFaker.Generate();

        // Act
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/payments")
        {
            Content = JsonContent.Create(paymentRequest)
        };
        requestMessage.Headers.Add("Idempotency-Key", idempotencyKey.ToString());

        var response = await client.SendAsync(requestMessage);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }
}

public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly bool _authorized;
    private readonly HttpStatusCode _httpStatusCode;

    public MockHttpMessageHandler(bool authorized = true, 
        HttpStatusCode httpStatusCode = HttpStatusCode.OK)
    {
        _authorized = authorized;
        _httpStatusCode = httpStatusCode;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var response = new BankPaymentResponse
        {
            Authorized = _authorized,
            AuthorizationCode = _authorized ? Guid.NewGuid().ToString() : ""
        };

        var jsonContent = System.Text.Json.JsonSerializer.Serialize(response);
        var responseMessage = new HttpResponseMessage(_httpStatusCode)
        {
            Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json")
        };

        return Task.FromResult(responseMessage);
    }
}