using FluentAssertions;

using FluentResults;

using Microsoft.Extensions.Logging;

using Moq;

using PaymentGateway.Api.Common.GuidGenerator;
using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Infrastructure.Clients.BankSimulator;
using PaymentGateway.Api.Models.Domain;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Repositories.Payment;
using PaymentGateway.Api.Services;
using PaymentGateway.Api.Tests.Helpers;

namespace PaymentGateway.Api.Tests.Unit;

public class PaymentServiceTests
{
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
    private readonly Mock<IBankSimulator> _bankSimulatorMock;
    private readonly Mock<IGuidGenerator> _guidGeneratorMock;
    private readonly Mock<ILogger<PaymentService>> _loggerMock;
    private readonly PaymentService _sut;

    public PaymentServiceTests()
    {
        _paymentRepositoryMock = new Mock<IPaymentRepository>();
        _bankSimulatorMock = new Mock<IBankSimulator>();
        _guidGeneratorMock = new Mock<IGuidGenerator>();
        _loggerMock = new Mock<ILogger<PaymentService>>();
        _sut = new PaymentService(_loggerMock.Object, _paymentRepositoryMock.Object, _bankSimulatorMock.Object,
            _guidGeneratorMock.Object);
    }

    #region GetPayment

    [Fact]
    public void GetPayment_WhenPaymentExists_ReturnsPayment()
    {
        // Arrange
        var existingPayment = PaymentTestDataBuilder.BuildPayment(PaymentStatus.Authorized);

        var expected = new PaymentResponse
        {
            Id = existingPayment.Id,
            Amount = existingPayment.Amount,
            Status = existingPayment.Status,
            CardNumberLastFour = existingPayment.CardNumberLastFour,
            ExpiryMonth = existingPayment.ExpiryMonth,
            ExpiryYear = existingPayment.ExpiryYear,
            Currency = existingPayment.Currency
        };

        _paymentRepositoryMock
            .Setup(x => x.Get(existingPayment.Id))
            .Returns(existingPayment);

        // Act
        var actual = _sut.GetPayment(existingPayment.Id);

        // Assert
        actual.Should().NotBeNull();
        actual.IsSuccess.Should().BeTrue();
        actual.Value.Should().BeOfType<PaymentResponse>();
        actual.Value.Should().BeEquivalentTo(expected);

        _paymentRepositoryMock.Verify(x => x.Get(existingPayment.Id), Times.Once);
    }

    [Fact]
    public void GetPayment_WhenPaymentDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        _paymentRepositoryMock.Setup(x => x.Get(paymentId)).Returns((Payment?)null);

        // Act
        var actual = _sut.GetPayment(paymentId);

        // Assert
        actual.IsFailed.Should().BeTrue();
        actual.Errors.Should().ContainSingle(e => e.Message == "Payment not found");
    }

    #endregion

    #region ProcessPayment

    [Fact]
    public async Task ProcessPayment_WhenBankSimulatorReturnsAuthorized_ReturnsAuthorizedResponse()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var idempotencyKey = Guid.NewGuid().ToString();
        var authorizationCode = Guid.NewGuid().ToString();
        var request = PaymentTestDataBuilder.BuildAuthorizedRequest();

        var expected = new PaymentResponse
        {
            Id = paymentId,
            CardNumberLastFour = int.Parse(request.CardNumber[^4..]),
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            Currency = request.Currency,
            Amount = request.Amount,
            Status = PaymentStatus.Authorized
        };

        _bankSimulatorMock
            .Setup(x => x.ProcessPaymentAsync(It.IsAny<BankSimulatorRequest>(), It.IsAny<Guid>()))
            .ReturnsAsync(Result.Ok(new BankSimulatorResponse
            {
                Authorized = true, AuthorizationCode = authorizationCode
            }));

        _guidGeneratorMock
            .Setup(x => x.NewGuid())
            .Returns(paymentId);

        // Act
        var actual = await _sut.ProcessPaymentAsync(request, idempotencyKey);

        // Assert
        actual.IsSuccess.Should().BeTrue();
        actual.Value.Should().BeEquivalentTo(expected);

        _paymentRepositoryMock.Verify(x => x.Add(It.IsAny<Payment>()), Times.Once);
    }

    [Fact]
    public async Task ProcessPayment_WhenBankSimulatorReturnsUnauthorized_ReturnsDeclinedResponse()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var idempotencyKey = Guid.NewGuid().ToString();
        var request = PaymentTestDataBuilder.BuildDeclinedRequest();

        var expected = new PaymentResponse
        {
            Id = paymentId,
            CardNumberLastFour = int.Parse(request.CardNumber[^4..]),
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            Currency = request.Currency,
            Amount = request.Amount,
            Status = PaymentStatus.Declined
        };

        _bankSimulatorMock
            .Setup(x => x.ProcessPaymentAsync(It.IsAny<BankSimulatorRequest>(), It.IsAny<Guid>()))
            .ReturnsAsync(Result.Ok(new BankSimulatorResponse
            {
                Authorized = false, AuthorizationCode = string.Empty
            }));

        _guidGeneratorMock
            .Setup(x => x.NewGuid())
            .Returns(paymentId);

        // Act
        var actual = await _sut.ProcessPaymentAsync(request, idempotencyKey);

        // Assert
        actual.IsSuccess.Should().BeTrue();
        actual.Value.Should().BeEquivalentTo(expected);

        _paymentRepositoryMock.Verify(x => x.Add(It.IsAny<Payment>()), Times.Once);
    }

    [Fact]
    public async Task ProcessPayment_WhenBankSimulatorIsUnavailable_ReturnsServiceUnavailableResult()
    {
        // Arrange
        var request = PaymentTestDataBuilder.BuildUnavailableRequest();

        _bankSimulatorMock
            .Setup(x => x.ProcessPaymentAsync(It.IsAny<BankSimulatorRequest>(), It.IsAny<Guid>()))
            .ReturnsAsync(Result.Fail("Bank simulator: Service Unavailable"));

        // Act
        var actual = await _sut.ProcessPaymentAsync(request, Guid.NewGuid().ToString());

        // Assert
        actual.IsFailed.Should().BeTrue();
        actual.Errors.Should().ContainSingle(e => e.Message.Contains("Service Unavailable"));
    }

    [Fact]
    public async Task ProcessPayment_WhenRequestIsInvalid_ReturnsRejectedResponse()
    {
        // Arrange
        var request = PaymentTestDataBuilder.BuildInvalidRequest();

        // Act
        var actual = await _sut.ProcessPaymentAsync(request, Guid.NewGuid().ToString());

        // Assert
        actual.IsSuccess.Should().BeFalse();

        var error = actual.Errors.OfType<PaymentError>().Single();
        error.ErrorType.Should().Be(PaymentErrorType.ValidationFailed);
        error.Message.Should().Be("Validation failed");
        error.Data.Should().BeAssignableTo<IEnumerable<string>>();
        ((IEnumerable<string>)error.Data!).Should().NotBeEmpty();

        _bankSimulatorMock.Verify(
            x => x.ProcessPaymentAsync(It.IsAny<BankSimulatorRequest>(), It.IsAny<Guid>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessPayment_WhenIdempotencyKeyAlreadyExists_ReturnsConflict()
    {
        // Arrange
        var idempotencyKey = Guid.NewGuid().ToString();
        var existingPayment = PaymentTestDataBuilder.BuildPayment(PaymentStatus.Authorized, idempotencyKey);

        _paymentRepositoryMock
            .Setup(x => x.GetByIdempotencyKey(idempotencyKey))
            .Returns(existingPayment);

        var request = PaymentTestDataBuilder.BuildAuthorizedRequest();

        // Act
        var actual = await _sut.ProcessPaymentAsync(request, idempotencyKey);

        // Assert
        actual.IsFailed.Should().BeTrue();
        actual.Errors.OfType<PaymentError>()
            .Should().ContainSingle(e => e.ErrorType == PaymentErrorType.Conflict);

        _bankSimulatorMock.Verify(
            x => x.ProcessPaymentAsync(It.IsAny<BankSimulatorRequest>(), It.IsAny<Guid>()),
            Times.Never);

        _paymentRepositoryMock.Verify(
            x => x.Add(It.IsAny<Payment>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessPayment_WhenSameCardDifferentIdempotencyKey_CreatesTwoPayments()
    {
        // Arrange
        var firstPaymentId = Guid.NewGuid();
        var secondPaymentId = Guid.NewGuid();
        var authorizationCode = Guid.NewGuid().ToString();
        var request = PaymentTestDataBuilder.BuildAuthorizedRequest();

        _bankSimulatorMock
            .Setup(x => x.ProcessPaymentAsync(It.IsAny<BankSimulatorRequest>(), It.IsAny<Guid>()))
            .ReturnsAsync(Result.Ok(new BankSimulatorResponse
            {
                Authorized = true, AuthorizationCode = authorizationCode
            }));

        _paymentRepositoryMock
            .Setup(x => x.GetByIdempotencyKey(It.IsAny<string>()))
            .Returns((Payment?)null);

        _guidGeneratorMock
            .SetupSequence(x => x.NewGuid())
            .Returns(firstPaymentId)
            .Returns(secondPaymentId);

        // Act
        var firstResult = await _sut.ProcessPaymentAsync(request, Guid.NewGuid().ToString());
        var secondResult = await _sut.ProcessPaymentAsync(request, Guid.NewGuid().ToString());

        // Assert
        firstResult.IsSuccess.Should().BeTrue();
        secondResult.IsSuccess.Should().BeTrue();
        firstResult.Value.Id.Should().NotBe(secondResult.Value.Id);

        _bankSimulatorMock.Verify(
            x => x.ProcessPaymentAsync(It.IsAny<BankSimulatorRequest>(), It.IsAny<Guid>()),
            Times.Exactly(2));

        _paymentRepositoryMock.Verify(
            x => x.Add(It.IsAny<Payment>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task ProcessPayment_WhenNoIdempotencyKey_ProcessesNormally()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var request = PaymentTestDataBuilder.BuildAuthorizedRequest();

        _bankSimulatorMock
            .Setup(x => x.ProcessPaymentAsync(It.IsAny<BankSimulatorRequest>(), It.IsAny<Guid>()))
            .ReturnsAsync(Result.Ok(new BankSimulatorResponse
            {
                Authorized = true, AuthorizationCode = Guid.NewGuid().ToString()
            }));

        _guidGeneratorMock
            .Setup(x => x.NewGuid())
            .Returns(paymentId);

        // Act
        var actual = await _sut.ProcessPaymentAsync(request, null);

        // Assert
        actual.IsSuccess.Should().BeTrue();

        _bankSimulatorMock.Verify(
            x => x.ProcessPaymentAsync(It.IsAny<BankSimulatorRequest>(), It.IsAny<Guid>()),
            Times.Once);

        _paymentRepositoryMock.Verify(
            x => x.Add(It.IsAny<Payment>()),
            Times.Once);
    }

    #endregion
}