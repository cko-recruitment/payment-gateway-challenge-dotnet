using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Api.Validation
{
    public class CardNumberValidationAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (string.IsNullOrEmpty(value?.ToString()) || !int.TryParse(value.ToString(), out _) || value.ToString().Length < 14 || value.ToString().Length > 19)
            {
                return false;
            }
            return true;
        }
    }
}
