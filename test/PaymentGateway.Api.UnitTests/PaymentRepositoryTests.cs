using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.UnitTests;

public class PaymentRepositoryTests
{
    [Fact]
    public void AddAndGet_ReturnsSamePayment()
    {
        // Arrange
        var repo = new PaymentsRepository();
        var payment = new PostPaymentResponse { Id = Guid.NewGuid() };

        // Act
        repo.Add(payment);
        var retrieved = repo.Get(payment.Id);

        // Assert
        Assert.Equal(payment, retrieved);
    }

    [Fact]
    public void Get_NonexistentId_ReturnsNull()
    {
        var repo = new PaymentsRepository();
        var result = repo.Get(Guid.NewGuid());
        Assert.Null(result);
    }
}