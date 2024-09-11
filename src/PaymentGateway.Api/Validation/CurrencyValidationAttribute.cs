using System.ComponentModel.DataAnnotations;

using PaymentGateway.Api.Constants.Enums;

namespace PaymentGateway.Api.Validation
{
    public class CurrencyValidationAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            // not null or exists in currencies
            return value == null || !Enum.TryParse<Currencies>(value.ToString(), out _) ? false : Enum.IsDefined(typeof(Currencies), value);
        }
    }
}
