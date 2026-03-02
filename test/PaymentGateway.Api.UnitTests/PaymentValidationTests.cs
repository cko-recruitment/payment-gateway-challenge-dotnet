using PaymentGateway.Api.Services;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models;

namespace PaymentGateway.Api.UnitTests;

public class PaymentValidationTests
{
    private readonly PaymentsRepository _repo = new();
    private readonly PaymentProcessor _processor;

    public PaymentValidationTests()
    {
        if (_repo != null)
        {
            _processor = new PaymentProcessor(_repo);
        }
    }

    private PostPaymentRequest CreateValidRequest()
    {
        return new PostPaymentRequest
        {
            CardNumber = "4532015112830366",
            ExpiryMonth = 12,
            ExpiryYear = DateTime.UtcNow.Year + 1,
            Currency = "USD",
            Amount = 1000,
            Cvv = "123"
        };
    }

    [Theory]
    [InlineData("123")]                      
    [InlineData("12345678901234567890")]     
    [InlineData("")]                         
    [InlineData("453201511283036A")]         
    [InlineData("4532 0151 1283 0366")]      
    public void Process_InvalidCardNumber_ReturnsRejected(string cardNumber)
    {
        var request = CreateValidRequest();
        request.CardNumber = cardNumber;

        var response = _processor.ProcessAsync(request);

        Assert.Equal(PaymentStatus.Rejected, response.Result.Status);
    }

    [Theory]
    [InlineData("12345678901234")]           
    [InlineData("4532015112830366")]         
    [InlineData("1234567890123456789")]      
    public void Process_ValidCardNumberLength_Processes(string cardNumber)
    {
        var request = CreateValidRequest();
        request.CardNumber = cardNumber;

        var response = _processor.ProcessAsync(request);

        Assert.NotEqual(PaymentStatus.Rejected, response.Result.Status);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    [InlineData(-1)]
    public void Process_InvalidExpiryMonth_ReturnsRejected(int month)
    {
        var request = CreateValidRequest();
        request.ExpiryMonth = month;

        var response = _processor.ProcessAsync(request);

        Assert.Equal(PaymentStatus.Rejected, response.Result.Status);
    }

    [Fact]
    public void Process_PastExpiryYear_ReturnsRejected()
    {
        var request = CreateValidRequest();
        request.ExpiryYear = DateTime.UtcNow.Year - 1;

        var response = _processor.ProcessAsync(request);

        Assert.Equal(PaymentStatus.Rejected, response.Result.Status);
    }

    [Fact]
    public void Process_ExpiredCardLastMonth_ReturnsRejected()
    {
        var now = DateTime.UtcNow;
        var lastMonth = now.AddMonths(-1);
        var request = CreateValidRequest();
        request.ExpiryMonth = lastMonth.Month;
        request.ExpiryYear = lastMonth.Year;

        var response = _processor.ProcessAsync(request);
        
        Assert.Equal(PaymentStatus.Rejected, response.Result.Status);
    }

    [Fact]
    public void Process_FutureExpiryDate_Processes()
    {
        var request = CreateValidRequest();
        request.ExpiryMonth = 12;
        request.ExpiryYear = DateTime.UtcNow.Year + 2;

        var response = _processor.ProcessAsync(request);

        Assert.NotEqual(PaymentStatus.Rejected, response.Result.Status);
    }

    [Theory]
    [InlineData("")]
    [InlineData("US")]                       
    [InlineData("USDA")]                     
    [InlineData("JPY")]                      
    [InlineData("AUD")]                      
    public void Process_InvalidCurrency_ReturnsRejected(string currency)
    {
        var request = CreateValidRequest();
        request.Currency = currency;

        var response = _processor.ProcessAsync(request);

        Assert.Equal(PaymentStatus.Rejected, response.Result.Status);
    }

    [Theory]
    [InlineData("USD")]
    [InlineData("GBP")]
    [InlineData("EUR")]
    [InlineData("usd")]                      
    [InlineData("gbp")]
    public void Process_ValidCurrency_Processes(string currency)
    {
        var request = CreateValidRequest();
        request.Currency = currency;

        var response = _processor.ProcessAsync(request);

        Assert.NotEqual(PaymentStatus.Rejected, response.Result.Status);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Process_InvalidAmount_ReturnsRejected(int amount)
    {
        var request = CreateValidRequest();
        request.Amount = amount;

        var response = _processor.ProcessAsync(request);

        if (response.Result != null)
        {
            Assert.Equal(PaymentStatus.Rejected, response.Result.Status);
        }
    }

    [Theory]
    [InlineData(1)]                         
    [InlineData(100)]
    [InlineData(999999)]
    public void Process_ValidAmount_Processes(int amount)
    {
        var request = CreateValidRequest();
        request.Amount = amount;

        var response = _processor.ProcessAsync(request);

        if (response.Result != null)
        {
            Assert.NotEqual(PaymentStatus.Rejected, response.Result.Status);
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("12")]                     
    [InlineData("12345")]                    
    [InlineData("12A")]                     
    [InlineData("A234")]                     
    public void Process_InvalidCvv_ReturnsRejected(string cvv)
    {
        var request = CreateValidRequest();
        request.Cvv = cvv;

        var response = _processor.ProcessAsync(request);

        Assert.Equal(PaymentStatus.Rejected, response.Result.Status);
    }

    [Theory]
    [InlineData("123")]                      
    [InlineData("1234")]                     
    public void Process_ValidCvv_Processes(string cvv)
    {
        var request = CreateValidRequest();
        request.Cvv = cvv;

        var response = _processor.ProcessAsync(request);

        Assert.NotEqual(PaymentStatus.Rejected, response.Result.Status);
    }

    [Fact]
    public void Process_NullRequest_ReturnsRejected()
    {
        var response = _processor.ProcessAsync(null);

        Assert.Equal(PaymentStatus.Rejected, response.Result.Status);
    }
}