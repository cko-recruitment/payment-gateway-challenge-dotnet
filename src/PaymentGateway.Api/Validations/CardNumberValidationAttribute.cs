using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Api.Validation
{
    public class CardNumberValidationAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            // length must be inclusive between 14 and 19 characters and only numbers
            var stringValue = value?.ToString();
            if (!string.IsNullOrEmpty(value?.ToString()) && long.TryParse(value.ToString(), out _) && stringValue?.Length >= 14 && stringValue?.Length <= 19)
            {
                return true;
            }
            return false;
        }
    }
}
