using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc.Testing;

using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.IntegrationTests;

public class PaymentsControllerTests
{
    [Fact]
    public async Task CreateAndRetrievePayment_Successfully()
    {
        // Arrange
        var factory = new WebApplicationFactory<PaymentsController>();
        var client = factory.CreateClient();

        var request = new PostPaymentRequest
        {
            Amount = 1000,
            Currency = "GBP",
            CardNumber = "4532015112830366",
            ExpiryMonth = 12,
            ExpiryYear = DateTime.UtcNow.Year + 1,
            Cvv = "123"
        };

        // Act - Create payment
        var createResponse = await client.PostAsJsonAsync("/api/Payments", request);
        var createdPayment = await createResponse.Content.ReadFromJsonAsync<PostPaymentResponse>();
        
        // Act - Retrieve payment
        var getResponse = await client.GetAsync($"/api/Payments/{createdPayment!.Id}");
        var retrievedPayment = await getResponse.Content.ReadFromJsonAsync<GetPaymentResponse>();
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.NotNull(retrievedPayment);
        Assert.Equal(createdPayment.Id, retrievedPayment.Id);
        Assert.Equal(createdPayment.Status, retrievedPayment.Status);
        Assert.Equal(366, retrievedPayment.CardNumberLastFour);
        Assert.Equal(1000, retrievedPayment.Amount);
        Assert.Equal("GBP", retrievedPayment.Currency);
    }

    [Fact]
    public async Task GetPayment_NotFound_Returns404()
    {
        // Arrange
        var factory = new WebApplicationFactory<PaymentsController>();
        var client = factory.CreateClient();
        
        // Act
        var response = await client.GetAsync($"/api/Payments/{Guid.NewGuid()}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreatePayment_InvalidData_ReturnsBadRequest()
    {
        // Arrange
        var factory = new WebApplicationFactory<PaymentsController>();
        var client = factory.CreateClient();

        var request = new PostPaymentRequest
        {
            Amount = 0,
            Currency = "",
            CardNumber = "123",
            ExpiryMonth = 1,
            ExpiryYear = 2000,
            Cvv = "12"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/Payments", request);
        var content = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(content);
        Assert.Equal(PaymentStatus.Rejected, content.Status);
    }

    [Fact]
    public async Task CreatePayment_ValidData_ReturnsSuccess()
    {
        // Arrange
        var factory = new WebApplicationFactory<PaymentsController>();
        var client = factory.CreateClient();

        var request = new PostPaymentRequest
        {
            Amount = 500,
            Currency = "GBP",
            CardNumber = "2221000000000009",
            ExpiryMonth = 11,
            ExpiryYear = DateTime.UtcNow.Year + 1,
            Cvv = "123"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/Payments", request);
        var created = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(created);
        Assert.Contains(created.Status, new[] { PaymentStatus.Authorized, PaymentStatus.Declined });
        Assert.Equal(9, created.CardNumberLastFour);
        Assert.Equal(500, created.Amount);
        Assert.Equal("GBP", created.Currency);
    }
}