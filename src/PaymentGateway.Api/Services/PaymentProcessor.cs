using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services;

public interface IPaymentProcessor
{
    Task<PostPaymentResponse?> ProcessAsync(PostPaymentRequest request);
}

public class PaymentProcessor : IPaymentProcessor
{
    private readonly PaymentsRepository _repository;
   
    public PaymentProcessor(PaymentsRepository repository)
    {
        _repository = repository;
    }

    public async Task<PostPaymentResponse?> ProcessAsync(PostPaymentRequest request)
    {
        var validationError = ValidatePaymentRequest(request);

        if (validationError != null)
        {
            return validationError;
        }

        PaymentStatus status;

        if (Convert.ToInt32(request.CardNumber[^1]) % 2 == 0)
            status = PaymentStatus.Declined;
        status = PaymentStatus.Authorized;

        var response = new PostPaymentResponse
        {
            Id = Guid.NewGuid(),
            Status = status,
            CardNumberLastFour = int.Parse(request.CardNumber.Substring(request.CardNumber.Length - 4)),
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            Currency = request.Currency,
            Amount = request.Amount
        };
        
        _repository.Add(response);
        return response;

    }
    private PostPaymentResponse? ValidatePaymentRequest(PostPaymentRequest request)
    {
        if (request == null)
        {
            return CreateRejectedResponse(request);
        }

        // Card number validation: Required, 14-19 characters, numeric only
        if (string.IsNullOrWhiteSpace(request.CardNumber) 
            || request.CardNumber.Length < 14 
            || request.CardNumber.Length > 19
            || !request.CardNumber.All(char.IsDigit))
        {
            return CreateRejectedResponse(request);
        }

        // Expiry month validation: Required, 1-12
        if (request.ExpiryMonth < 1 || request.ExpiryMonth > 12)
        {
            return CreateRejectedResponse(request);
        }

        // Expiry year validation: Required, must be in future
        var now = DateTime.UtcNow;
        if (request.ExpiryYear < now.Year)
        {
            return CreateRejectedResponse(request);
        }

        // Expiry date combination must be in the future
        var expiryDate = new DateTime(request.ExpiryYear, request.ExpiryMonth, 1).AddMonths(1).AddDays(-1);
        if (expiryDate < now)
        {
            return CreateRejectedResponse(request);
        }

        // Currency validation: Required, exactly 3 characters, valid ISO code
        var validCurrencies = new[] { "USD", "GBP", "EUR" };
        if (string.IsNullOrWhiteSpace(request.Currency) 
            || request.Currency.Length != 3
            || !validCurrencies.Contains(request.Currency.ToUpper()))
        {
            return CreateRejectedResponse(request);
        }

        // Amount validation: Required, must be positive integer
        if (request.Amount <= 0)
        {
            return CreateRejectedResponse(request);
        }

        // CVV validation: Required, 3-4 characters, numeric only
        if (string.IsNullOrWhiteSpace(request.Cvv)
            || request.Cvv.Length < 3
            || request.Cvv.Length > 4
            || !request.Cvv.All(char.IsDigit))
        {
            return CreateRejectedResponse(request);
        }

        return null; // validation passed
    }
    
    private PostPaymentResponse CreateRejectedResponse(PostPaymentRequest? request)
    {
        int lastFour = ExtractCardNumberLastFour(request);
        return new PostPaymentResponse
        {
            Id = Guid.NewGuid(),
            Status = PaymentStatus.Rejected,
            CardNumberLastFour = lastFour,
            ExpiryMonth = request?.ExpiryMonth ?? 0,
            ExpiryYear = request?.ExpiryYear ?? 0,
            Currency = request?.Currency ?? string.Empty,
            Amount = request?.Amount ?? 0
        };
    }

    private static int ExtractCardNumberLastFour(PostPaymentRequest? request)
    {
        var lastFour = 0;
        if (request?.CardNumber != null && request.CardNumber.Length >= 4 && request.CardNumber.All(char.IsDigit))
        {
            lastFour = int.Parse(request.CardNumber.Substring(request.CardNumber.Length - 4));
        }

        return lastFour;
    }
}
