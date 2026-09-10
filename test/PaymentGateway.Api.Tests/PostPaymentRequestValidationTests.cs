using System.ComponentModel.DataAnnotations;

using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Tests;

public class PostPaymentRequestValidationTests
{
    private static PostPaymentRequest CreateValidRequest() => new()
    {
        CardNumber = "2222405343248877",
        ExpiryMonth = 4,
        ExpiryYear = DateTime.UtcNow.Year + 1,
        Currency = "GBP",
        Amount = 100,
        Cvv = "123"
    };

    private static bool TryValidate(PostPaymentRequest request, out List<ValidationResult> results)
    {
        results = new List<ValidationResult>();
        var context = new ValidationContext(request);
        return Validator.TryValidateObject(request, context, results, validateAllProperties: true);
    }

    [Fact]
    public void ValidRequest_PassesValidation()
    {
        Assert.True(TryValidate(CreateValidRequest(), out _));
    }

    [Theory]
    [InlineData("123")] // too short
    [InlineData("12345678901234567890")] // too long
    [InlineData("2222abc343248877")] // non-numeric
    public void InvalidCardNumber_FailsValidation(string cardNumber)
    {
        var request = CreateValidRequest();
        request.CardNumber = cardNumber;

        Assert.False(TryValidate(request, out _));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public void InvalidExpiryMonth_FailsValidation(int expiryMonth)
    {
        var request = CreateValidRequest();
        request.ExpiryMonth = expiryMonth;

        Assert.False(TryValidate(request, out _));
    }

    [Fact]
    public void ExpiryDateInThePast_FailsValidation()
    {
        var request = CreateValidRequest();
        request.ExpiryMonth = 1;
        request.ExpiryYear = DateTime.UtcNow.Year - 1;

        Assert.False(TryValidate(request, out _));
    }

    [Fact]
    public void UnsupportedCurrency_FailsValidation()
    {
        var request = CreateValidRequest();
        request.Currency = "JPY";

        Assert.False(TryValidate(request, out _));
    }

    [Theory]
    [InlineData("12")]
    [InlineData("12345")]
    [InlineData("abc")]
    public void InvalidCvv_FailsValidation(string cvv)
    {
        var request = CreateValidRequest();
        request.Cvv = cvv;

        Assert.False(TryValidate(request, out _));
    }

    [Fact]
    public void ZeroAmount_FailsValidation()
    {
        var request = CreateValidRequest();
        request.Amount = 0;

        Assert.False(TryValidate(request, out _));
    }
}
