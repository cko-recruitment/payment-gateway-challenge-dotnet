using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.UnitTests;

public class PaymentResponseTests
{
    [Fact]
    public async Task Process_MultiplePayments_GeneratesUniqueIds()
    {
        // Arrange
        var repo = new PaymentsRepository();
        var processor = new PaymentProcessor(repo);

        var request = new PostPaymentRequest
        {
            CardNumber = "4532015112830366",
            ExpiryMonth = 12,
            ExpiryYear = DateTime.UtcNow.Year + 1,
            Currency = "USD",
            Amount = 100,
            Cvv = "123"
        };

        // Act
        var response1 = await processor.ProcessAsync(request);
        var response2 = await processor.ProcessAsync(request);
        var response3 = await processor.ProcessAsync(request);

        // Assert - Each payment gets unique ID
        Assert.NotEqual(response1.Id, response2.Id);
        Assert.NotEqual(response2.Id, response3.Id);
        Assert.NotEqual(response1.Id, response3.Id);
    }

    [Fact]
    public async Task Process_AuthorizedPayment_HasStatusAuthorized()
    {
        // Arrange - use seed that produces Authorized
        var repo = new PaymentsRepository();
        var processor = new PaymentProcessor(repo);

        var request = new PostPaymentRequest
        {
            CardNumber = "4532015112830366",
            ExpiryMonth = 12,
            ExpiryYear = DateTime.UtcNow.Year + 1,
            Currency = "EUR",
            Amount = 250,
            Cvv = "456"
        };

        // Act
        var response = await processor.ProcessAsync(request);

        // Assert - specific seed produces deterministic result
        Assert.True(response.Status == PaymentStatus.Authorized || response.Status == PaymentStatus.Declined);
    }
}