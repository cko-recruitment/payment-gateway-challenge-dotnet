using PaymentGateway.Api.Constants.Enums;
using PaymentGateway.Api.Validation;

namespace PaymentGateway.Api.Tests.Validations
{
    public class CurrencyValidationTests
    {
        private readonly CurrencyValidationAttribute _currencyValidationAttribute;

        public CurrencyValidationTests()
        {
            _currencyValidationAttribute = new CurrencyValidationAttribute();
        }

        [Fact]
        public void IsValid_WhenCurrencyIsValid_ReturnsTrue()
        {
            // Arrange
            foreach (var currency in Enum.GetValues(typeof(Currencies)))
            {
                // Act
                var result = _currencyValidationAttribute.IsValid(currency.ToString());

                // Assert
                Assert.True(result);
            }
        }

        [Fact]
        public void IsValid_WhenCurrencyIsInvalid_ReturnsFalse()
        {
            // Arrange
            var invalidCurrency = "INVALID";

            // Act
            var result = _currencyValidationAttribute.IsValid(invalidCurrency);

            // Assert
            Assert.False(result);
        }
    }
}
