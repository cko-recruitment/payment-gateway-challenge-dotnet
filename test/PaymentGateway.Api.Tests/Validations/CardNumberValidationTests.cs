using PaymentGateway.Api.Validation;

namespace PaymentGateway.Api.Tests.Validations
{
    public class CardNumberValidationTests
    {
        private readonly CardNumberValidationAttribute _cardNumberValidationAttribute;
        public CardNumberValidationTests()
        {
            _cardNumberValidationAttribute = new CardNumberValidationAttribute();
        }

        [Theory]
        [InlineData("12345678901234", true)]
        [InlineData("1234567890123456789", true)]
        [InlineData("abcdefghijklmnopqr", false)]
        [InlineData("123456789012345abc", false)]
        [InlineData("123456789012345", true)]
        public void CardNumber_IsValid_ReturnsExpected(string cardNumber, bool expected)
        {
            // Act
            var result = _cardNumberValidationAttribute.IsValid(cardNumber);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
