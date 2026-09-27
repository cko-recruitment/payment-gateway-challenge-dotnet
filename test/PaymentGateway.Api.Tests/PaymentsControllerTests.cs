using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Tests;

public class PaymentsControllerTests
{
    private readonly Random _random = new();
    
    [Fact]
    public async Task Payment_is_created_successfully()
    {
        // Arrange
        var request = GetValidRequest();
        var webApplicationFactory = CreateFactory();
        var repository = webApplicationFactory.Services.GetRequiredService<PaymentsRepository>();
        var client = webApplicationFactory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/Payments", request);
        var paymentResponse = await response.Content.ReadFromJsonAsync<PaymentResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(request.CardNumber[^4..], paymentResponse!.CardNumberLastFour);
        Assert.Equal(request.ExpiryMonth, paymentResponse.ExpiryMonth);
        Assert.Equal(request.ExpiryYear, paymentResponse.ExpiryYear);
        Assert.Equal(request.Currency, paymentResponse.Currency);
        Assert.Equal(request.Amount, paymentResponse.Amount);
        Assert.Equal("Authorized", paymentResponse.Status.ToString());
        
        // verify that the payment was stored in the repository
        Assert.Single(repository.Payments);
        var payment = repository.Payments[0];
        Assert.Equal(paymentResponse.Id, payment.Id);
        Assert.Equal(paymentResponse.CardNumberLastFour, payment.CardNumber[^4..]);
        Assert.Equal(paymentResponse.ExpiryMonth, payment.ExpiryMonth);
        Assert.Equal(paymentResponse.ExpiryYear, payment.ExpiryYear);
        Assert.Equal(paymentResponse.Currency, payment.Currency);
        Assert.Equal(paymentResponse.Amount, payment.Amount);
    }

    [Fact]
    public async Task Invalid_request_returns_bad_request_with_validation_problem_details()
    {
        // Arrange
        var request = GetValidRequest(); 
        request.CardNumber = "123"; // Invalid card number
        var webApplicationFactory = CreateFactory();
        var client = webApplicationFactory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/Payments", request);
        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problemDetails);
        Assert.Equal("Payment request is invalid.", problemDetails!.Title);
        Assert.Equal(StatusCodes.Status400BadRequest, problemDetails.Status);
    }
    
    [Fact]
    public async Task Payment_status_is_declined_when_authorization_is_declined_by_bank()
    {
        // Arrange
        var request = GetValidRequest();
        request.CardNumber = "4000000000000002"; // Simulate a card that will be declined
        var webApplicationFactory = CreateFactory();
        var client = webApplicationFactory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/Payments", request);
        var paymentResponse = await response.Content.ReadFromJsonAsync<PaymentResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("Declined", paymentResponse!.Status.ToString());
    }
    

    [Fact]
    public async Task Payment_creation_fails_with_InternalServerError_when_bank_returns_503()
    {
        // Arrange
        var request = GetValidRequest();
        request.CardNumber = "5000000000000000"; // Simulate a card that will cause bank to return 503
        var webApplicationFactory = CreateFactory();
        var repository = webApplicationFactory.Services.GetRequiredService<PaymentsRepository>();
        var client = webApplicationFactory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/Payments", request);
        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.NotNull(problemDetails);
        Assert.Equal("An unexpected error occurred while processing the payment.", problemDetails!.Title);
        Assert.Equal(StatusCodes.Status500InternalServerError, problemDetails.Status);
        
        Assert.Empty(repository.Payments);  // Ensure no payment was stored in the repository
    }
    

    [Fact]
    public async Task Stored_payment_can_be_retrieved()
    {
        // Arrange
        var payment = new PaymentModel
        {
            Id = Guid.NewGuid(),
            ExpiryYear = _random.Next(2023, 2030),
            ExpiryMonth = _random.Next(1, 12),
            Amount = _random.Next(1, 10000),
            CardNumber = "123412341234" + _random.Next(1111, 9999),
            Currency = "GBP",
            BankAuthorization = new BankAuthorization(PaymentStatus.Authorized)
        };

        var paymentsRepository = new PaymentsRepository();
        paymentsRepository.Add(payment);

        var webApplicationFactory = CreateFactory();
        var client = webApplicationFactory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services => ((ServiceCollection)services)
                .AddSingleton(paymentsRepository)))
            .CreateClient();

        // Act
        var response = await client.GetAsync($"/api/Payments/{payment.Id}");
        var paymentResponse = await response.Content.ReadFromJsonAsync<PaymentResponse>();
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(paymentResponse);
    }

    [Fact]
    public async Task GetPayment_Returns_404_when_payment_is_not_found()
    {
        // Arrange
        var webApplicationFactory = CreateFactory();
        var client = webApplicationFactory.CreateClient();
        
        // Act
        var response = await client.GetAsync($"/api/Payments/{Guid.NewGuid()}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    
    
    // Helper methods
    private static WebApplicationFactory<PaymentsController> CreateFactory(string environment = "Development") =>
        new WebApplicationFactory<PaymentsController>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment(environment);
            builder.ConfigureLogging(logging => logging.ClearProviders());
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<TimeProvider>(new FixedTimeProvider());
            });
        });
    
    private static CreatePaymentRequest GetValidRequest() => new()
    {
        CardNumber = "1234567890121235",
        ExpiryMonth = 10,
        ExpiryYear = 2026,
        Currency = "GBP",
        Amount = 100,
        Cvv = "123"
    };
    
    private sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(2026, 9, 26, 12, 0, 0, TimeSpan.Zero);
    }
}
