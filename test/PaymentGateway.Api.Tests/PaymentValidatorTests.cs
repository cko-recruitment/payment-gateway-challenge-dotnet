using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Tests;

public class PaymentValidatorTests
{
    private static readonly DateTime Today = new(2026, 9, 26, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Valid_request_is_accepted()
    {
        Assert.Null(PaymentValidator.Validate(ValidRequest(), Today));
    }

    [Fact]
    public void CardNumber_is_required()
    {
        CreatePaymentRequest request = ValidRequest();
        request.CardNumber = null!;

        AssertInvalid(request, nameof(request.CardNumber), "Card number is required.");
    }

    [Fact]
    public void CardNumber_is_not_empty()
    {
        CreatePaymentRequest request = ValidRequest();
        request.CardNumber = string.Empty;

        AssertInvalid(request, nameof(request.CardNumber), "Card number must contain 14 to 19 digits.");
    }

    [Theory]
    [InlineData("1234567890123")]
    [InlineData("12345678901234567890")]
    public void CardNumber_length_is_between_14_and_19(string cardNumber)
    {
        CreatePaymentRequest request = ValidRequest();
        request.CardNumber = cardNumber;

        AssertInvalid(request, nameof(request.CardNumber), "Card number must contain 14 to 19 digits.");
    }

    [Theory]
    [InlineData("1234567890123a")]
    [InlineData("1234567890123 ")]
    [InlineData("1234-5678-9012-3456")]
    public void CardNumber_contains_only_digits(string cardNumber)
    {
        CreatePaymentRequest request = ValidRequest();
        request.CardNumber = cardNumber;

        AssertInvalid(request, nameof(request.CardNumber), "Card number must contain only digits.");
    }

    [Theory]
    [InlineData(14)]
    [InlineData(15)]
    [InlineData(16)]
    [InlineData(17)]
    [InlineData(18)]
    [InlineData(19)]
    public void CardNumber_with_valid_length_and_digits_is_accepted(int length)
    {
        CreatePaymentRequest request = ValidRequest();
        request.CardNumber = new string('0', length);

        Assert.Null(PaymentValidator.Validate(request, Today));
    }

    [Fact]
    public void ExpiryMonth_is_required()
    {
        CreatePaymentRequest request = ValidRequest();
        request.ExpiryMonth = null;

        AssertInvalid(request, nameof(request.ExpiryMonth), "Expiry month is required.");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(13)]
    public void ExpiryMonth_is_between_1_and_12(int month)
    {
        CreatePaymentRequest request = ValidRequest();
        request.ExpiryMonth = month;
        request.ExpiryYear = 2027;

        AssertInvalid(request, nameof(request.ExpiryMonth), "Expiry month must be between 1 and 12.");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(12)]
    public void ExpiryMonth_at_valid_boundaries_is_accepted(int month)
    {
        CreatePaymentRequest request = ValidRequest();
        request.ExpiryMonth = month;
        request.ExpiryYear = 2027;

        Assert.Null(PaymentValidator.Validate(request, Today));
    }

    [Fact]
    public void ExpiryYear_is_required()
    {
        CreatePaymentRequest request = ValidRequest();
        request.ExpiryYear = null;

        AssertInvalid(request, nameof(request.ExpiryYear), "Expiry year is required.");
    }

    [Theory]
    [InlineData(2026, 8)]
    [InlineData(2025, 12)]
    public void ExpiryYear_and_month_are_not_in_the_past(int year, int month)
    {
        CreatePaymentRequest request = ValidRequest();
        request.ExpiryYear = year;
        request.ExpiryMonth = month;

        AssertInvalid(request, nameof(request.ExpiryYear), "Expiry month and year must not be in the past.");
    }

    [Theory]
    [InlineData(2026, 10)]
    [InlineData(2027, 1)]
    public void ExpiryYear_and_month_in_the_future_are_accepted(int year, int month)
    {
        CreatePaymentRequest request = ValidRequest();
        request.ExpiryYear = year;
        request.ExpiryMonth = month;

        Assert.Null(PaymentValidator.Validate(request, Today));
    }

    [Fact]
    public void ExpiryYear_rollover_from_December_to_January_is_accepted()
    {
        CreatePaymentRequest request = ValidRequest();
        request.ExpiryYear = 2027;
        request.ExpiryMonth = 1;

        Assert.Null(PaymentValidator.Validate(request, new DateTime(2026, 12, 31)));
    }

    [Theory]
    [InlineData(-357911911)]
    [InlineData(int.MinValue)]
    public void Negative_expiry_year_is_rejected_without_overflow(int year)
    {
        CreatePaymentRequest request = ValidRequest();
        request.ExpiryYear = year;

        AssertInvalid(request, nameof(request.ExpiryYear), "Expiry month and year must not be in the past.");
    }

    [Fact]
    public void Expiry_in_the_current_month_is_rejected()
    {
        CreatePaymentRequest request = ValidRequest();
        request.ExpiryYear = Today.Year;
        request.ExpiryMonth = Today.Month;

        AssertInvalid(request, nameof(request.ExpiryYear), "Expiry month and year must not be in the past.");
    }

    [Fact]
    public void Currency_is_required()
    {
        CreatePaymentRequest request = ValidRequest();
        request.Currency = null!;

        AssertInvalid(request, nameof(request.Currency), "Currency is required.");
    }

    [Fact]
    public void Currency_is_not_empty()
    {
        CreatePaymentRequest request = ValidRequest();
        request.Currency = string.Empty;

        AssertInvalid(request, nameof(request.Currency), "Currency is not supported.");
    }

    [Theory]
    [InlineData("GB")]
    [InlineData("GBPP")]
    [InlineData("GBP ")]
    public void Currency_length_is_3(string currency)
    {
        CreatePaymentRequest request = ValidRequest();
        request.Currency = currency;

        AssertInvalid(request, nameof(request.Currency), "Currency is not supported.");
    }

    [Theory]
    [InlineData("JPY")]
    [InlineData("XYZ")]
    [InlineData("123")]
    public void Currency_is_one_of_the_supported_codes(string currency)
    {
        CreatePaymentRequest request = ValidRequest();
        request.Currency = currency;

        AssertInvalid(request, nameof(request.Currency), "Currency is not supported.");
    }

    [Theory]
    [InlineData("GBP")]
    [InlineData("USD")]
    [InlineData("EUR")]
    public void Currency_with_supported_code_is_accepted(string currency)
    {
        CreatePaymentRequest request = ValidRequest();
        request.Currency = currency;

        Assert.Null(PaymentValidator.Validate(request, Today));
    }

    [Fact]
    public void Amount_is_required()
    {
        CreatePaymentRequest request = ValidRequest();
        request.Amount = null;

        AssertInvalid(request, nameof(request.Amount), "Amount is required.");
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    [InlineData(0)]
    public void Amount_is_greater_than_zero(int amount)
    {
        CreatePaymentRequest request = ValidRequest();
        request.Amount = amount;

        AssertInvalid(request, nameof(request.Amount), "Amount is required and must be a positive integer in minor units.");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(1050)]
    [InlineData(int.MaxValue)]
    public void Amount_with_positive_integer_value_is_accepted(int amount)
    {
        CreatePaymentRequest request = ValidRequest();
        request.Amount = amount;

        Assert.Null(PaymentValidator.Validate(request, Today));
    }

    [Fact]
    public void Cvv_is_required()
    {
        CreatePaymentRequest request = ValidRequest();
        request.Cvv = null;

        AssertInvalid(request, nameof(request.Cvv), "CVV is required.");
    }

    [Fact]
    public void Cvv_is_not_empty()
    {
        CreatePaymentRequest request = ValidRequest();
        request.Cvv = string.Empty;

        AssertInvalid(request, nameof(request.Cvv), "CVV must contain 3 or 4 digits.");
    }

    [Theory]
    [InlineData("12")]
    [InlineData("12345")]
    public void Cvv_length_is_3_or_4(string cvv)
    {
        CreatePaymentRequest request = ValidRequest();
        request.Cvv = cvv;

        AssertInvalid(request, nameof(request.Cvv), "CVV must contain 3 or 4 digits.");
    }

    [Theory]
    [InlineData("12a")]
    [InlineData("1 2")]
    [InlineData("123 ")]
    public void Cvv_contains_only_digits(string cvv)
    {
        CreatePaymentRequest request = ValidRequest();
        request.Cvv = cvv;

        AssertInvalid(request, nameof(request.Cvv), "CVV must contain only digits.");
    }

    [Theory]
    [InlineData("123")]
    [InlineData("1234")]
    [InlineData("001")]
    [InlineData("0001")]
    public void Cvv_with_valid_length_and_digits_is_accepted(string cvv)
    {
        CreatePaymentRequest request = ValidRequest();
        request.Cvv = cvv;

        Assert.Null(PaymentValidator.Validate(request, Today));
    }

    private static void AssertInvalid(CreatePaymentRequest request, string field, params string[] messages)
    {
        ValidationError? error = PaymentValidator.Validate(request, Today);

        Assert.NotNull(error);
        Assert.Equal(field, error!.Field);
        Assert.Equal(messages, error.Messages);
    }

    private static CreatePaymentRequest ValidRequest() => new()
    {
        CardNumber = "1234567890123456",
        ExpiryMonth = 10,
        ExpiryYear = 2026,
        Currency = "GBP",
        Amount = 100,
        Cvv = "123"
    };
}