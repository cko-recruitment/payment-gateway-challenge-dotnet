namespace PaymentGateway.Api.Models.Responses;

public class PostPaymentResponse
{
    public Guid Id { get; set; }
    public PaymentStatus Status { get; set; }
    public int? CardNumberLastFour { get; set; }
    public int? ExpiryMonth { get; set; }
    public int? ExpiryYear { get; set; }
    public string? Currency { get; set; }
    public int? Amount { get; set; }
}

public class PostPaymentResponseBuilder()
{
    private readonly PostPaymentResponse _paymentResponse = new PostPaymentResponse();

    public PostPaymentResponseBuilder WithNewId()
    {
        _paymentResponse.Id = Guid.NewGuid();
        return this;
    }
    public PostPaymentResponseBuilder WithStatus(PaymentStatus status)
    {
        _paymentResponse.Status = status;
        return this;
    }

    public PostPaymentResponseBuilder WithCardNumberLastFour(long? cardNumber)
    {
        if (cardNumber == null)
        {
            _paymentResponse.CardNumberLastFour = null;
            return this;
        }

        var cardNoString = cardNumber.ToString();

        if (cardNoString.Length < 4)
        {
            _paymentResponse.CardNumberLastFour = null;
            return this;
        }

        var lastFourString = cardNoString.Substring(cardNoString.Length - 4);

        if (Int32.TryParse(lastFourString, out var lastFour))
        {
            _paymentResponse.CardNumberLastFour = lastFour;
        }
        else
        {
            _paymentResponse.CardNumberLastFour = null;
        }

        return this;
    }

    public PostPaymentResponseBuilder WithExpiryMonth(int? month)
    {
        _paymentResponse.ExpiryMonth = month;
        return this;
    }

    public PostPaymentResponseBuilder WithExpiryYear(int? year)
    {
        _paymentResponse.ExpiryYear = year;
        return this;
    }

    public PostPaymentResponseBuilder WithCurrency(string? currency)
    {
        _paymentResponse.Currency = currency;
        return this;
    }

    public PostPaymentResponseBuilder WithAmount(int? amount)
    {
        _paymentResponse.Amount = amount;
        return this;
    }

    public PostPaymentResponse Build()
    {
        return _paymentResponse;
    }
}

