using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using FluentResults;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Infrastructure.Clients.BankSimulator;
using PaymentGateway.Api.Models.Domain;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Repositories.Payment;
using PaymentGateway.Api.Services;
using PaymentGateway.Api.Tests.Helpers;

namespace PaymentGateway.Api.Tests.Integration;

public class PaymentsControllerTests
{
    #region GetPayment

    [Fact]
    public async Task GetPayment_WhenPaymentExists_ReturnsPayment()
    {
        // Arrange
        var payment = PaymentTestDataBuilder.BuildPayment(PaymentStatus.Authorized);

        var paymentRepository = new PaymentsRepository();
        paymentRepository.Add(payment);

        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.WithWebHostBuilder(builder =>
                builder.ConfigureServices(services => ((ServiceCollection)services)
                    .AddSingleton<IPaymentRepository>(paymentRepository)))
            .CreateClient();

        // Act
        var response = await client.GetAsync($"/api/Payments/{payment.Id}");
        var paymentResponse = await response.Content.ReadFromJsonAsync<PaymentResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        paymentResponse.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPayment_WhenPaymentDoesNotExist_Returns404()
    {
        // Arrange
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/Payments/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region ProcessPayment

    [Fact]
    public async Task ProcessPayment_WhenBankReturnsAuthorized_ReturnsAuthorizedResponse()
    {
        // Arrange
        var authorizationCode = Guid.NewGuid().ToString();
        var request = PaymentTestDataBuilder.BuildAuthorizedRequest();

        var bankSimulatorMock = new Mock<IBankSimulator>();
        bankSimulatorMock
            .Setup(x => x.ProcessPaymentAsync(It.IsAny<BankSimulatorRequest>(), It.IsAny<Guid>()))
            .ReturnsAsync(Result.Ok(new BankSimulatorResponse
            {
                Authorized = true, AuthorizationCode = authorizationCode
            }));

        var paymentRepository = new PaymentsRepository();
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();

        var client = webApplicationFactory.WithWebHostBuilder(builder =>
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(bankSimulatorMock.Object);
                    services.AddSingleton<IPaymentRepository>(paymentRepository);
                }))
            .CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/Payments", request);
        var paymentResponse = await response.Content.ReadFromJsonAsync<PaymentResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        paymentResponse.Should().NotBeNull();
        paymentResponse!.Status.Should().Be(PaymentStatus.Authorized);
        paymentResponse.CardNumberLastFour.Should().Be(int.Parse(request.CardNumber[^4..]));

        var savedPayment = paymentRepository.Get(paymentResponse.Id);
        savedPayment.Should().NotBeNull();
        savedPayment!.Id.Should().Be(paymentResponse.Id);
        savedPayment.Status.Should().Be(paymentResponse.Status);
    }

    [Fact]
    public async Task ProcessPayment_WhenBankReturnsDeclined_ReturnsDeclinedResponse()
    {
        // Arrange
        var request = PaymentTestDataBuilder.BuildDeclinedRequest();

        var bankSimulatorMock = new Mock<IBankSimulator>();
        bankSimulatorMock
            .Setup(x => x.ProcessPaymentAsync(It.IsAny<BankSimulatorRequest>(), It.IsAny<Guid>()))
            .ReturnsAsync(Result.Ok(new BankSimulatorResponse
            {
                Authorized = false, AuthorizationCode = string.Empty
            }));

        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.WithWebHostBuilder(builder =>
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(bankSimulatorMock.Object);
                }))
            .CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/Payments", request);
        var paymentResponse = await response.Content.ReadFromJsonAsync<PaymentResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        paymentResponse.Should().NotBeNull();
        paymentResponse!.Status.Should().Be(PaymentStatus.Declined);
        paymentResponse.CardNumberLastFour.Should().Be(int.Parse(request.CardNumber[^4..]));
    }

    [Fact]
    public async Task ProcessPayment_WhenBankSimulatorIsUnavailable_Returns503AndDoesNotStore()
    {
        // Arrange
        var request = PaymentTestDataBuilder.BuildUnavailableRequest();

        var bankSimulatorMock = new Mock<IBankSimulator>();
        bankSimulatorMock
            .Setup(x => x.ProcessPaymentAsync(It.IsAny<BankSimulatorRequest>(), It.IsAny<Guid>()))
            .ReturnsAsync(Result.Fail("Bank simulator: Service Unavailable"));

        var paymentRepositoryMock = new Mock<IPaymentRepository>();

        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.WithWebHostBuilder(builder =>
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(bankSimulatorMock.Object);
                    services.AddSingleton(paymentRepositoryMock.Object);
                }))
            .CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/Payments", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        paymentRepositoryMock.Verify(x => x.Add(It.IsAny<Payment>()), Times.Never);
    }

    [Fact]
    public async Task ProcessPayment_WhenRequestIsInvalid_ReturnsRejectedResponse()
    {
        // Arrange
        var request = PaymentTestDataBuilder.BuildInvalidRequest();

        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/Payments", request);
        var paymentResponse = await response.Content.ReadFromJsonAsync<IEnumerable<string>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        paymentResponse.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ProcessPayment_WhenSameIdempotencyKeySentTwice_ReturnsConflictOnSecondRequest()
    {
        // Arrange
        var idempotencyKey = Guid.NewGuid().ToString();
        var request = PaymentTestDataBuilder.BuildAuthorizedRequest();

        var bankSimulatorMock = new Mock<IBankSimulator>();
        bankSimulatorMock
            .Setup(x => x.ProcessPaymentAsync(It.IsAny<BankSimulatorRequest>(), It.IsAny<Guid>()))
            .ReturnsAsync(Result.Ok(new BankSimulatorResponse
            {
                Authorized = true, AuthorizationCode = Guid.NewGuid().ToString()
            }));

        var paymentRepository = new PaymentsRepository();
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();

        var client = webApplicationFactory.WithWebHostBuilder(builder =>
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(bankSimulatorMock.Object);
                    services.AddSingleton<IPaymentRepository>(paymentRepository);
                }))
            .CreateClient();

        var requestMessage1 = new HttpRequestMessage(HttpMethod.Post, "/api/Payments");
        requestMessage1.Headers.Add("Idempotency-Key", idempotencyKey);
        requestMessage1.Content = JsonContent.Create(request);

        var requestMessage2 = new HttpRequestMessage(HttpMethod.Post, "/api/Payments");
        requestMessage2.Headers.Add("Idempotency-Key", idempotencyKey);
        requestMessage2.Content = JsonContent.Create(request);

        // Act
        var firstResponse = await client.SendAsync(requestMessage1);
        var secondResponse = await client.SendAsync(requestMessage2);

        var firstPayment = await firstResponse.Content.ReadFromJsonAsync<PaymentResponse>();
        var errorResponse = await secondResponse.Content.ReadAsStringAsync();

        // Assert — first request succeeds
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        firstPayment.Should().NotBeNull();
        firstPayment!.Status.Should().Be(PaymentStatus.Authorized);

        // Second request conflicts
        secondResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
        errorResponse.Should().NotBeNullOrEmpty();
        errorResponse.Should().Contain("A payment with this idempotency key already exists");

        bankSimulatorMock.Verify(
            x => x.ProcessPaymentAsync(It.IsAny<BankSimulatorRequest>(), It.IsAny<Guid>()),
            Times.Once);
    }

    #endregion
}