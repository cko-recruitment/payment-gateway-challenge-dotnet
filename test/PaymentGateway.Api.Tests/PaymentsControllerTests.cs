using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Models.BankSimulator;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Repositories;
using PaymentGateway.Api.Services;
using PaymentGateway.Api.Validators;

namespace PaymentGateway.Api.Tests;

public class PaymentsControllerTests
{
    [Fact]
    public async Task CreatesAuthorizedPaymentSuccessfully()
    {
        var repository = new PaymentsRepository();
        var bankClient = new MockedBankSimulatorClient(new BankPaymentResponse { Authorized = true });
        var controller = CreateController(bankClient, repository);

        var result = await controller.PostPaymentAsync(CreateValidRequest("4111111111111111"), CancellationToken.None);

        var createdResult = Assert.IsType<CreatedAtRouteResult>(result.Result);
        var response = Assert.IsType<PostPaymentResponse>(createdResult.Value);

        Assert.Equal(PaymentStatus.Authorized, response.Status);
        Assert.Equal("1111", response.CardNumberLastFour);
        Assert.Equal(1, bankClient.CallCount);
        Assert.NotNull(repository.Get(response.Id));
    }

    [Fact]
    public async Task CreatesDeclinedPaymentSuccessfully()
    {
        var bankClient = new MockedBankSimulatorClient(new BankPaymentResponse { Authorized = false });
        var controller = CreateController(bankClient, new PaymentsRepository());

        var result = await controller.PostPaymentAsync(CreateValidRequest("4111111111111112"), CancellationToken.None);

        var createdResult = Assert.IsType<CreatedAtRouteResult>(result.Result);
        var response = Assert.IsType<PostPaymentResponse>(createdResult.Value);

        Assert.Equal(PaymentStatus.Declined, response.Status);
        Assert.Equal("1112", response.CardNumberLastFour);
        Assert.Equal(1, bankClient.CallCount);
    }

    [Fact]
    public async Task RejectsInvalidPaymentBeforeCallingBankSimulator()
    {
        var bankClient = new MockedBankSimulatorClient(new BankPaymentResponse { Authorized = true });
        var controller = CreateController(bankClient, new PaymentsRepository());

        var result = await controller.PostPaymentAsync(CreateValidRequest("123"), CancellationToken.None);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<PostPaymentResponse>(badRequestResult.Value);

        Assert.Equal(PaymentStatus.Rejected, response.Status);
        Assert.Contains("Card number must be between 14 and 19 characters long.", response.Errors);
        Assert.Equal(0, bankClient.CallCount);
    }

    [Fact]
    public async Task RejectsMissingPaymentRequestBeforeCallingBankSimulator()
    {
        var bankClient = new MockedBankSimulatorClient(new BankPaymentResponse { Authorized = true });
        var controller = CreateController(bankClient, new PaymentsRepository());

        var result = await controller.PostPaymentAsync(null, CancellationToken.None);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<PostPaymentResponse>(badRequestResult.Value);
        Assert.Equal(PaymentStatus.Rejected, response.Status);
        Assert.Contains("Payment request is required.", response.Errors);
        Assert.Equal(0, bankClient.CallCount);
    }

    [Fact]
    public async Task Returns503WhenBankSimulatorIsUnavailable()
    {
        var bankClient = new MockedBankSimulatorClient(null);
        var controller = CreateController(bankClient, new PaymentsRepository());

        var result = await controller.PostPaymentAsync(CreateValidRequest("4111111111111110"), CancellationToken.None);

        var statusCodeResult = Assert.IsType<StatusCodeResult>(result.Result);
        Assert.Equal(503, statusCodeResult.StatusCode);
        Assert.Equal(1, bankClient.CallCount);
    }

    [Fact]
    public async Task RetrievesAPaymentSuccessfully()
    {
        var repository = new PaymentsRepository();
        var controller = CreateController(new MockedBankSimulatorClient(new BankPaymentResponse()), repository);
        var createResult = await controller.PostPaymentAsync(CreateValidRequest("4111111111111111"), CancellationToken.None);
        var createdResult = Assert.IsType<CreatedAtRouteResult>(createResult.Result);
        var createdPayment = Assert.IsType<PostPaymentResponse>(createdResult.Value);

        var result = controller.GetPaymentAsync(createdPayment.Id);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var paymentResponse = Assert.IsType<GetPaymentResponse>(okResult.Value);
        Assert.Equal(createdPayment.Id, paymentResponse.Id);
        Assert.Equal("1111", paymentResponse.CardNumberLastFour);
    }

    [Fact]
    public void Returns404IfPaymentNotFound()
    {
        var controller = CreateController(new MockedBankSimulatorClient(new BankPaymentResponse()), new PaymentsRepository());

        var result = controller.GetPaymentAsync(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result.Result);
    }

    // Helpers
    private static PaymentsController CreateController(
        IBankSimulatorClient bankClient,
        IPaymentsRepository repository)
    {
        var paymentService = new PaymentService(bankClient, repository, new PostPaymentRequestValidator());
        return new PaymentsController(NullLogger<PaymentsController>.Instance, paymentService);
    }

    private static PostPaymentRequest CreateValidRequest(string cardNumber)
    {
        return new PostPaymentRequest
        {
            CardNumber = cardNumber,
            ExpiryMonth = 12,
            ExpiryYear = DateTime.UtcNow.Year + 1,
            Currency = "GBP",
            Amount = 100,
            Cvv = "123"
        };
    }

    private sealed class MockedBankSimulatorClient : IBankSimulatorClient
    {
        private readonly BankPaymentResponse? _response;

        public MockedBankSimulatorClient(BankPaymentResponse? response)
        {
            _response = response;
        }

        //Check whether the bank client was called
        public int CallCount { get; private set; }

        public Task<BankPaymentResponse?> ProcessPaymentAsync(
            BankPaymentRequest request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(_response);
        }
    }
}
