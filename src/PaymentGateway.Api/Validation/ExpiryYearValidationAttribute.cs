using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Api.Validation
{
    public class ExpiryYearValidationAttribute : ValidationAttribute
    {
        public ExpiryYearValidationAttribute()
        {
            ErrorMessage = "The expiry year must be equal to or greater than the current year";
        }

        public override bool IsValid(object value)
        {
            if (value is int year)
            {
                return year >= DateTime.Now.Year;
                
            }
            return false;
        }
    }
}
