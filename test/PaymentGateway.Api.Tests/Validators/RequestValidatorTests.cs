using FluentAssertions;
using NSubstitute;
using PaymentGateway.Api.Exceptions;
using PaymentGateway.Api.Services.Interfaces;
using PaymentGateway.Api.Tests.Fakers;
using PaymentGateway.Api.Validators;

namespace PaymentGateway.Api.Tests.Validators;

public class RequestValidatorTests
{
    private readonly IPaymentsRepository _paymentsRepository;
    private readonly PostPaymentRequestValidator _postPaymentRequestValidator;
    private readonly RequestValidator _requestValidator;
    private readonly PostPaymentRequestFaker _paymentRequestFaker = new();

    public RequestValidatorTests()
    {
        _paymentsRepository = Substitute.For<IPaymentsRepository>();
        _postPaymentRequestValidator = new PostPaymentRequestValidator();
        _requestValidator = new RequestValidator(_paymentsRepository, _postPaymentRequestValidator);
    }

    [Fact]
    public void Validate_DoesNotThrow_WhenRequestIsValid()
    {
        // Arrange
        var idempotencyKey = Guid.NewGuid().ToString();
        var paymentRequest = _paymentRequestFaker.Generate();

        _paymentsRepository.Exists(Arg.Any<Guid>()).Returns(false);

        // Act
        var action = () => _requestValidator.Validate(paymentRequest, idempotencyKey);

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void Validate_ThrowsPaymentValidationException_WhenIdempotencyKeyIsNotGuid()
    {
        // Arrange
        var idempotencyKey = "not-a-guid";
        var paymentRequest = _paymentRequestFaker.Generate();

        // Act
        var action = () => _requestValidator.Validate(paymentRequest, idempotencyKey);

        // Assert
        action.Should().Throw<PaymentValidationException>()
            .WithMessage("Payment validation failed")
            .And.Errors.Should().ContainKey("Idempotency-Key")
            .WhoseValue.Should().Contain(x => x.Contains("Guid"));
    }

    [Fact]
    public void Validate_ThrowsDuplicatePaymentException_WhenIdempotencyKeyAlreadyExists()
    {
        // Arrange
        var idempotencyKey = Guid.NewGuid();
        var paymentRequest = _paymentRequestFaker.Generate();

        _paymentsRepository.Exists(Arg.Any<Guid>()).Returns(true);

        // Act
        var action = () => _requestValidator.Validate(paymentRequest, idempotencyKey.ToString());

        // Assert
        action.Should().Throw<DuplicatePaymentException>()
            .WithMessage($"Payment with idempotency key {idempotencyKey} already exists");
    }

    [Fact]
    public void Validate_ThrowsPaymentValidationException_WhenCardNumberIsInvalid()
    {
        // Arrange
        var idempotencyKey = Guid.NewGuid().ToString();
        var paymentRequest = _paymentRequestFaker
            .WithCardNumber("123")
            .Generate();

        _paymentsRepository.Exists(Arg.Any<Guid>()).Returns(false);

        // Act
        var action = () => _requestValidator.Validate(paymentRequest, idempotencyKey);

        // Assert
        action.Should().Throw<PaymentValidationException>()
            .And.Errors.Should().ContainKey("CardNumber");
    }

    [Fact]
    public void Validate_ThrowsPaymentValidationException_WhenExpiryMonthIsInvalid()
    {
        // Arrange
        var idempotencyKey = Guid.NewGuid().ToString();
        var paymentRequest = _paymentRequestFaker
            .WithMonth(13)  // Invalid: > 12
            .Generate();

        _paymentsRepository.Exists(Arg.Any<Guid>()).Returns(false);

        // Act
        var action = () => _requestValidator.Validate(paymentRequest, idempotencyKey);

        // Assert
        action.Should().Throw<PaymentValidationException>()
            .And.Errors.Should().ContainKey("ExpiryMonth");
    }

    [Fact]
    public void Validate_ThrowsPaymentValidationException_WhenExpiryDateIsInThePast()
    {
        // Arrange
        var idempotencyKey = Guid.NewGuid().ToString();
        var currentYear = DateTime.UtcNow.Year;
        var paymentRequest = _paymentRequestFaker
            .WithYear(currentYear - 1)  // Past year
            .WithMonth(12)
            .Generate();

        _paymentsRepository.Exists(Arg.Any<Guid>()).Returns(false);

        // Act
        var action = () => _requestValidator.Validate(paymentRequest, idempotencyKey);

        // Assert
        action.Should().Throw<PaymentValidationException>()
            .And.Errors.Should().ContainKey("ExpiryDate");
    }

    [Fact]
    public void Validate_ThrowsPaymentValidationException_WhenCurrencyIsNotAllowed()
    {
        // Arrange
        var idempotencyKey = Guid.NewGuid().ToString();
        var paymentRequest = _paymentRequestFaker
            .WithCurrency("JPY")  // Not in allowed list
            .Generate();

        _paymentsRepository.Exists(Arg.Any<Guid>()).Returns(false);

        // Act
        var action = () => _requestValidator.Validate(paymentRequest, idempotencyKey);

        // Assert
        action.Should().Throw<PaymentValidationException>()
            .And.Errors.Should().ContainKey("Currency");
    }

    [Fact]
    public void Validate_ThrowsPaymentValidationException_WhenCvvIsInvalid()
    {
        // Arrange
        var idempotencyKey = Guid.NewGuid().ToString();
        var paymentRequest = _paymentRequestFaker
            .WithCvv("12")  // Too short (must be 3-4)
            .Generate();

        _paymentsRepository.Exists(Arg.Any<Guid>()).Returns(false);

        // Act
        var action = () => _requestValidator.Validate(paymentRequest, idempotencyKey);

        // Assert
        action.Should().Throw<PaymentValidationException>()
            .And.Errors.Should().ContainKey("Cvv");
    }

    [Fact]
    public void Validate_ThrowsPaymentValidationException_WhenMultipleValidationErrorsOccur()
    {
        // Arrange
        var idempotencyKey = Guid.NewGuid().ToString();
        var paymentRequest = _paymentRequestFaker
            .WithCardNumber("123") // Invalid: too short
            .WithMonth(13)         // Invalid: > 12
            .WithCurrency("JPY")   // Invalid: not allowed
            .WithCvv("2")          // Invalid: too short
            .Generate();

        _paymentsRepository.Exists(Arg.Any<Guid>()).Returns(false);

        // Act
        var action = () => _requestValidator.Validate(paymentRequest, idempotencyKey);

        // Assert
        action.Should().Throw<PaymentValidationException>()
            .And.Errors.Should()
            .HaveCountGreaterThanOrEqualTo(4)
            .And.ContainKeys("CardNumber", "ExpiryMonth", "Currency", "Cvv");
    }

    [Fact]
    public void Validate_ThrowsPaymentValidationException_WhenCardNumberContainsNonNumericCharacters()
    {
        // Arrange
        var idempotencyKey = Guid.NewGuid().ToString();
        var paymentRequest = _paymentRequestFaker
            .WithCardNumber("4532015112830abc")  // Contains letters
            .Generate();

        _paymentsRepository.Exists(Arg.Any<Guid>()).Returns(false);

        // Act
        var action = () => _requestValidator.Validate(paymentRequest, idempotencyKey);

        // Assert
        action.Should().Throw<PaymentValidationException>()
            .And.Errors.Should().ContainKey("CardNumber");
    }

    [Fact]
    public void Validate_ThrowsPaymentValidationException_WhenCvvContainsNonNumericCharacters()
    {
        // Arrange
        var idempotencyKey = Guid.NewGuid().ToString();
        var paymentRequest = _paymentRequestFaker
            .WithCvv("12a")  // Contains letter
            .Generate();

        _paymentsRepository.Exists(Arg.Any<Guid>()).Returns(false);

        // Act
        var action = () => _requestValidator.Validate(paymentRequest, idempotencyKey);

        // Assert
        action.Should().Throw<PaymentValidationException>()
            .And.Errors.Should().ContainKey("Cvv");
    }

    [Theory]
    [InlineData("GBP")]
    [InlineData("USD")]
    [InlineData("EUR")]
    public void Validate_DoesNotThrow_WhenCurrencyIsAllowed(string currency)
    {
        // Arrange
        var idempotencyKey = Guid.NewGuid().ToString();
        var paymentRequest = _paymentRequestFaker
            .WithCurrency(currency)
            .Generate();

        _paymentsRepository.Exists(Arg.Any<Guid>()).Returns(false);

        // Act
        var action = () => _requestValidator.Validate(paymentRequest, idempotencyKey);

        // Assert
        action.Should().NotThrow();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(6)]
    [InlineData(12)]
    public void Validate_DoesNotThrow_WhenExpiryMonthIsValid(int month)
    {
        // Arrange
        var idempotencyKey = Guid.NewGuid().ToString();
        var currentYear = DateTime.UtcNow.Year;
        var paymentRequest = _paymentRequestFaker
            .WithMonth(month)
            .WithYear(currentYear + 2)
            .Generate();

        _paymentsRepository.Exists(Arg.Any<Guid>()).Returns(false);

        // Act
        var action = () => _requestValidator.Validate(paymentRequest, idempotencyKey);

        // Assert
        action.Should().NotThrow();
    }
}