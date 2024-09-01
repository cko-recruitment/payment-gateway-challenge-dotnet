using PaymentGateway.Api.Interfaces;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Tests
{
    public class PaymentRepositoryTests
    {
        private readonly IPaymentsRepository sut = new PaymentsRepository();

        [Fact]
        public void Add_AddNewPayment_AddsSuccessfully()
        {
            // Arrange 
            var payment = new PostPaymentResponse();
            var id = Guid.NewGuid();
            payment.Id = id;

            // Act 
            sut.Add(payment);
            var result = sut.Get(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
        }

        [Fact]
        public void Get_GetNonExistingPayment_IsNull()
        {
            // Arrange
            var id = Guid.NewGuid();

            // Act
            var result = sut.Get(id);

            // Assert
            Assert.Null(result);
        }
    }

}
