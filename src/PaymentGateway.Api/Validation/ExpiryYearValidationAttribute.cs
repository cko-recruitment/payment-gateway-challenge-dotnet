using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Api.Validation
{
    public class ExpiryYearValidationAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value != null && value is int year)
            {
                return year >= DateTime.Now.Year;
            }
            return false;
        }
    }
}
