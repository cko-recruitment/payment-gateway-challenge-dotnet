using FluentAssertions;

using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Validation;

namespace PaymentGateway.Api.Tests.Unit;

public class PostPaymentRequestValidatorTests
{
    private readonly PostPaymentRequestValidator _sut = new();

    private static PostPaymentRequest CreateValidRequest() => new()
    {
        CardNumber = "1234567890121111",
        ExpiryMonth = 12,
        ExpiryYear = DateTime.UtcNow.Year + 1,
        Currency = "GBP",
        Amount = 100,
        CVV = "123"
    };

    [Fact]
    public void Validate_ValidRequest_ReturnsSuccess()
    {
        var request = CreateValidRequest();
        var result = _sut.Validate(request);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("12345678901234")] // 14 chars
    [InlineData("12345678901234567")] // 17 chars
    [InlineData("1234567890123456789")] // 19 chars
    public void Validate_ValidCardNumber_ReturnsSuccess(string cardNumber)
    {
        var request = CreateValidRequest();
        request.CardNumber = cardNumber;

        var result = _sut.Validate(request);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().NotContain(x => x.PropertyName == nameof(PostPaymentRequest.CardNumber));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("1234567890123")] // 13 chars (too short)
    [InlineData("12345678901234567890")] // 20 chars (too long)
    [InlineData("123456789012ABCD")] // Non-numeric
    public void Validate_InvalidCardNumber_ReturnsFailure(string? cardNumber)
    {
        var request = CreateValidRequest();
        request.CardNumber = cardNumber ?? string.Empty;

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(PostPaymentRequest.CardNumber));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(6)]
    [InlineData(12)]
    public void Validate_ValidExpiryMonth_ReturnsSuccess(int expiryMonth)
    {
        var request = CreateValidRequest();
        request.ExpiryMonth = expiryMonth;

        var result = _sut.Validate(request);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().NotContain(x => x.PropertyName == nameof(PostPaymentRequest.ExpiryMonth));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public void Validate_InvalidExpiryMonth_ReturnsFailure(int expiryMonth)
    {
        var request = CreateValidRequest();
        request.ExpiryMonth = expiryMonth;

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(PostPaymentRequest.ExpiryMonth));
    }

    [Fact]
    public void Validate_PastExpiryYear_ReturnsFailure()
    {
        var request = CreateValidRequest();
        request.ExpiryYear = DateTime.UtcNow.Year - 1;

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(PostPaymentRequest.ExpiryYear));
    }

    [Fact]
    public void Validate_PastExpiryDate_ReturnsFailure()
    {
        var now = DateTime.UtcNow;
        var request = CreateValidRequest();
        request.ExpiryYear = now.Year;
        request.ExpiryMonth = 3;

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();

        result.Errors.Should().Contain(x => x.ErrorMessage == "Card expiry date must be in the future");
    }

    [Theory]
    [InlineData("USD")]
    [InlineData("EUR")]
    [InlineData("GBP")]
    [InlineData("usd")]
    [InlineData("gbp")]
    public void Validate_ValidCurrency_ReturnsSuccess(string currency)
    {
        var request = CreateValidRequest();
        request.Currency = currency;

        var result = _sut.Validate(request);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().NotContain(x => x.PropertyName == nameof(PostPaymentRequest.Currency));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("GB")]
    [InlineData("GBPP")]
    [InlineData("AUD")] // Not in the allowed list
    public void Validate_InvalidCurrency_ReturnsFailure(string? currency)
    {
        var request = CreateValidRequest();
        request.Currency = currency ?? string.Empty;

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(PostPaymentRequest.Currency));
    }

    [Theory]
    [InlineData(100)]
    public void Validate_ValidAmount_ReturnsSuccess(int amount)
    {
        var request = CreateValidRequest();
        request.Amount = amount;

        var result = _sut.Validate(request);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().NotContain(x => x.PropertyName == nameof(PostPaymentRequest.Amount));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_InvalidAmount_ReturnsFailure(int amount)
    {
        var request = CreateValidRequest();
        request.Amount = amount;

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(PostPaymentRequest.Amount));
    }

    [Theory]
    [InlineData("123")]
    [InlineData("1234")]
    public void Validate_ValidCVV_ReturnsSuccess(string cvv)
    {
        var request = CreateValidRequest();
        request.CVV = cvv;

        var result = _sut.Validate(request);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().NotContain(x => x.PropertyName == nameof(PostPaymentRequest.CVV));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("12")]
    [InlineData("12345")]
    [InlineData("ABC")]
    public void Validate_InvalidCVV_ReturnsFailure(string? cvv)
    {
        var request = CreateValidRequest();
        request.CVV = cvv ?? string.Empty;

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(PostPaymentRequest.CVV));
    }
}