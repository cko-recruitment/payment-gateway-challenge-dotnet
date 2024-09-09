using System.ComponentModel.DataAnnotations;

using PaymentGateway.Api.Constants.Enums;

namespace PaymentGateway.Api.Validation
{
    public class CurrencyValidationAttribute : ValidationAttribute
    {
        public CurrencyValidationAttribute()
        {
            ErrorMessage = "The currency input provided is not supported";
        }
        public override bool IsValid(object value)
        {
            if (Enum.IsDefined(typeof(Currencies), value))
            {
                return true;
            }
            return false;
        }
    }
}
