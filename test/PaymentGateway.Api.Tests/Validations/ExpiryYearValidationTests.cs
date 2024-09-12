using PaymentGateway.Api.Validation;

namespace PaymentGateway.Api.Tests.Validations
{
    public class ExpiryYearValidationTests
    {
        private readonly ExpiryYearValidationAttribute _expiryYearValidationAttribute;
        public ExpiryYearValidationTests()
        {
            _expiryYearValidationAttribute = new ExpiryYearValidationAttribute();
        }

        [Fact]
        public void IsValid_WhenExpiryYearIsValid_ReturnsTrue()
        {
            // Arrange
            var currentYear = DateTime.Now.Year;

            for (int i = 0; i < 3; i++)
            {
                // Act
                var result = _expiryYearValidationAttribute.IsValid(currentYear + i);

                // Assert
                Assert.True(result);
            }
        }

        [Fact]
        public void IsValid_WhenExpiryYearIsInvalid_ReturnsFalse()
        {
            // Arrange
            var currentYear = DateTime.Now.Year;
            var invalidExpiryYear = currentYear - 5;

            // Act
            var result = _expiryYearValidationAttribute.IsValid(invalidExpiryYear);

            // Assert
            Assert.False(result);
        }
    }
}
