using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Tests
{

    public class PaymentRequestValidatorTests()
    {
        private static readonly IIsoCodeProvider provider = new IsoCodeValidator();
        private readonly PaymentRequestValidator sut = new PaymentRequestValidator(provider);

#region Valid Test Cases

        [Theory]
        [InlineData(12345678912345)]
        [InlineData(1234567891234512391)]
        public void ValidRequest_ValidCardNumber_ReturnsTrue(long cardNumber)
        {
            // Arrange
            var request = new PostPaymentRequest
            {
                Amount = 100,
                CardNumber = cardNumber,
                Currency = "GBP",
                Cvv = 123,
                ExpiryMonth = 1,
                ExpiryYear = 25,
            };

            // Act 
            bool result = sut.Validate(request, out var validatedPostPayment);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(100)]
        [InlineData(60000)]
        public void ValidRequest_ValidAmount_ReturnsTrue(int amount)
        {
            // Arrange
            var request = new PostPaymentRequest
            {
                Amount = amount,
                CardNumber = 12345678912345,
                Currency = "GBP",
                Cvv = 123,
                ExpiryMonth = 1,
                ExpiryYear = 25,
            };

            // Act 
            bool result = sut.Validate(request, out var validatedPostPayment);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData("GBP")]
        [InlineData("USD")]
        [InlineData("EUR")]
        [InlineData("gbp")]
        [InlineData("usd")]
        [InlineData("eur")]
        public void ValidRequest_ValidCurrency_ReturnsTrue(string currency)
        {
            // Arrange
            var request = new PostPaymentRequest
            {
                Amount = 100,
                CardNumber = 12345678912345,
                Currency = currency,
                Cvv = 123,
                ExpiryMonth = 1,
                ExpiryYear = 25,
            };

            // Act 
            bool result = sut.Validate(request, out var validatedPostPayment);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(123)]
        [InlineData(1234)]
        public void ValidRequest_ValidCvv_ReturnsTrue(int cvv)
        {
            // Arrange
            var request = new PostPaymentRequest
            {
                Amount = 100,
                CardNumber = 12345678912345,
                Currency = "GBP",
                Cvv = cvv,
                ExpiryMonth = 1,
                ExpiryYear = 25,
            };

            // Act 
            bool result = sut.Validate(request, out var validatedPostPayment);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(12)]
        public void ValidRequest_ValidMonth_ReturnsTrue(int month)
        {
            // Arrange
            var request = new PostPaymentRequest
            {
                Amount = 100,
                CardNumber = 12345678912345,
                Currency = "GBP",
                Cvv = 123,
                ExpiryMonth = month,
                ExpiryYear = 25,
            };

            // Act 
            bool result = sut.Validate(request, out var validatedPostPayment);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(26)]
        [InlineData(30)]
        public void ValidRequest_ValidYear_ReturnsTrue(int year)
        {
            // Arrange
            var request = new PostPaymentRequest
            {
                Amount = 100,
                CardNumber = 12345678912345,
                Currency = "GBP",
                Cvv = 123,
                ExpiryMonth = 1,
                ExpiryYear = year,
            };

            // Act 
            bool result = sut.Validate(request, out var validatedPostPayment);

            // Assert
            Assert.True(result);
        }

#endregion Valid Test Cases

#region Invalid Test Cases

        [Theory]
        [InlineData(1)]
        public void InvalidRequest_InvalidCardNumber_ReturnsFalse(long cardNumber)
        {
            // Arrange
            var request = new PostPaymentRequest
            {
                Amount = 100,
                CardNumber = cardNumber,
                Currency = "GBP",
                Cvv = 123,
                ExpiryMonth = 1,
                ExpiryYear = 25,
            };

            // Act 
            bool result = sut.Validate(request, out var validatedPostPayment);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(12345)]
        public void InvalidRequest_InvalidCvv_ReturnsFalse(int cvv)
        {
            // Arrange
            var request = new PostPaymentRequest
            {
                Amount = 100,
                CardNumber = 123456789101112,
                Currency = "GBP",
                Cvv = cvv,
                ExpiryMonth = 1,
                ExpiryYear = 25,
            };

            // Act 
            bool result = sut.Validate(request, out var validatedPostPayment);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(13)]
        public void InvalidRequest_InvalidExpiryMonth_ReturnsFalse(int expiryMonth)
        {
            // Arrange
            var request = new PostPaymentRequest
            {
                Amount = 100,
                CardNumber = 123456789101112,
                Currency = "GBP",
                Cvv = 123,
                ExpiryMonth = expiryMonth,
                ExpiryYear = 25,
            };

            // Act 
            bool result = sut.Validate(request, out var validatedPostPayment);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(19)]
        public void InvalidRequest_InvalidExpiryYear_ReturnsFalse(int expiryYear)
        {
            // Arrange
            var request = new PostPaymentRequest
            {
                Amount = 100,
                CardNumber = 123456789101112,
                Currency = "GBP",
                Cvv = 123,
                ExpiryMonth = 1,
                ExpiryYear = expiryYear,
            };

            // Act 
            bool result = sut.Validate(request, out var validatedPostPayment);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]

        public void InvalidRequest_InvalidAmount_ReturnsFalse(int amount)
        {
            // Arrange
            var request = new PostPaymentRequest
            {
                Amount = amount,
                CardNumber = 123456789101112,
                Currency = "GBP",
                Cvv = 123,
                ExpiryMonth = 1,
                ExpiryYear = 25,
            };

            // Act 
            bool result = sut.Validate(request, out var validatedPostPayment);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("ABC")]
        [InlineData("abc")]

        public void InvalidRequest_InvalidCurrency_ReturnsFalse(string currency)
        {
            // Arrange
            var request = new PostPaymentRequest
            {
                Amount = 100,
                CardNumber = 123456789101112,
                Currency = currency,
                Cvv = 123,
                ExpiryMonth = 1,
                ExpiryYear = 25,
            };

            // Act 
            bool result = sut.Validate(request, out var validatedPostPayment);

            // Assert
            Assert.False(result);
        }

        #endregion Invalid Test Cases 
    }
}
