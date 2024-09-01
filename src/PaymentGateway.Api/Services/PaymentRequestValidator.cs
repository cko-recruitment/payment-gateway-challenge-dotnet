using System.Collections.Frozen;
using System.ComponentModel.DataAnnotations;
using PaymentGateway.Api.Interfaces;
using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Services
{
    /// <inheritdoc />
    public class PaymentRequestValidator(IIsoCodeValidator isoCodeValidator) : IPaymentRequestValidator
    {
        /// <inheritdoc />
        public bool Validate(PostPaymentRequest request, out ValidatedPostPaymentRequest validatedPostPayment)
        {
            validatedPostPayment = new();
            if (!HasRequiredValues(request)) 
            {
                return false;
            }
                     
            if (!isoCodeValidator.IsValidIsoCode(request.Currency))
            {
                return false;
            }
            validatedPostPayment.Currency = request.Currency;

            if (!CorrectCardLength((long)request.CardNumber))
            {
                return false;
            }
            validatedPostPayment.CardNumber = (long)request.CardNumber;

            if (!ValidExpiryDate((int)request.ExpiryMonth, (int)request.ExpiryYear))
            {
                return false;
            }
            validatedPostPayment.ExpiryMonth = (int)request.ExpiryMonth;
            validatedPostPayment.ExpiryYear = (int)request.ExpiryYear;

            if (!ValidCvv((int)request.Cvv))
            {
                return false;
            }
            validatedPostPayment.Cvv = (int)request.Cvv;

            validatedPostPayment.Amount = (int)request.Amount;

            return true;
        }

        private static bool CorrectCardLength(long cardNumber)
        {
            var ccLength = (int)Math.Floor(Math.Log10(cardNumber) + 1);
            if (ccLength < 14 || ccLength > 19)
            {
                return false;
            }
            return true;
        }

        private static bool HasRequiredValues(PostPaymentRequest request)
        {
            foreach (var property in request.GetType().GetProperties().Where(
                prop => Attribute.IsDefined(prop, typeof(RequiredAttribute))))
            {
                if (property.GetValue(request) is null)
                {
                    return false;
                }
            }
            return true;
        }

        private static bool ValidExpiryDate(int month, int year)
        {
            if (month < 1 || month > 12)
            {
                return false;
            }

            DateTime expiry = new DateTime(year, month, 1);

            if (expiry < DateTime.UtcNow)
            {
                return false;
            }
            
            return true;
        }

        private static bool ValidCvv(int cvv)
        {
            int cvvLength = (int)Math.Floor(Math.Log10(cvv) + 1);
            return (cvvLength.Equals(3) || cvvLength.Equals(4));

        }
    }
}


    public interface IIsoCodeValidator
    {
        public bool IsValidIsoCode(string isoCode);
    }


    public class IsoCodeValidator : IIsoCodeValidator
    {
        private static readonly FrozenSet<string> _allowedIsoList;

        static IsoCodeValidator()
        {
            _allowedIsoList = new[] {
                "usd",
                "gbp",
                "eur"}.ToFrozenSet();
        }

        public bool IsValidIsoCode(string isoCode)
        {
            if (isoCode.Length != 3)
            {
                return false;
            }
            return _allowedIsoList.Contains(isoCode, StringComparer.InvariantCultureIgnoreCase);
    }
}
