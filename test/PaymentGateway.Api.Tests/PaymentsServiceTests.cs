using Moq;

using PaymentGateway.Api.Enums;
using PaymentGateway.Api.Models.Bank;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Tests;

public class PaymentsServiceTests
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

    [Fact]
    public async Task ProcessPaymentAsync_StoresAndReturnsAuthorizedPayment_WhenBankAuthorizes()
    {
        // Arrange
        var bankClientMock = new Mock<IBankClient>();
        bankClientMock
            .Setup(x => x.ProcessPaymentAsync(It.IsAny<BankPaymentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BankPaymentResult.Success(true, "auth-code"));

        var repository = new PaymentsRepository();
        var service = new PaymentsService(bankClientMock.Object, repository);

        // Act
        var result = await service.ProcessPaymentAsync(CreateValidRequest());

        // Assert
        Assert.False(result.BankUnavailable);
        Assert.NotNull(result.Response);
        Assert.Equal(PaymentStatus.Authorized, result.Response!.Status);
        Assert.Equal("8877", result.Response.CardNumberLastFour);
        Assert.NotNull(repository.Get(result.Response.Id));
    }

    [Fact]
    public async Task ProcessPaymentAsync_ReturnsDeclined_WhenBankDeclines()
    {
        // Arrange
        var bankClientMock = new Mock<IBankClient>();
        bankClientMock
            .Setup(x => x.ProcessPaymentAsync(It.IsAny<BankPaymentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BankPaymentResult.Success(false, null));

        var service = new PaymentsService(bankClientMock.Object, new PaymentsRepository());

        // Act
        var result = await service.ProcessPaymentAsync(CreateValidRequest());

        // Assert
        Assert.False(result.BankUnavailable);
        Assert.Equal(PaymentStatus.Declined, result.Response!.Status);
    }

    [Fact]
    public async Task ProcessPaymentAsync_DoesNotStorePayment_WhenBankIsUnavailable()
    {
        // Arrange
        var bankClientMock = new Mock<IBankClient>();
        bankClientMock
            .Setup(x => x.ProcessPaymentAsync(It.IsAny<BankPaymentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BankPaymentResult.Unavailable());

        var repository = new PaymentsRepository();
        var service = new PaymentsService(bankClientMock.Object, repository);

        // Act
        var result = await service.ProcessPaymentAsync(CreateValidRequest());

        // Assert
        Assert.True(result.BankUnavailable);
        Assert.Null(result.Response);
    }

    [Fact]
    public void GetPayment_ReturnsNull_WhenPaymentDoesNotExist()
    {
        // Arrange
        var service = new PaymentsService(Mock.Of<IBankClient>(), new PaymentsRepository());

        // Act
        var result = service.GetPayment(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetPayment_ReturnsPayment_AfterItHasBeenProcessed()
    {
        // Arrange
        var bankClientMock = new Mock<IBankClient>();
        bankClientMock
            .Setup(x => x.ProcessPaymentAsync(It.IsAny<BankPaymentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BankPaymentResult.Success(true, "auth-code"));

        var service = new PaymentsService(bankClientMock.Object, new PaymentsRepository());
        var processed = await service.ProcessPaymentAsync(CreateValidRequest());

        // Act
        var result = service.GetPayment(processed.Response!.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(processed.Response.Id, result!.Id);
        Assert.Equal(processed.Response.Status, result.Status);
    }
}
