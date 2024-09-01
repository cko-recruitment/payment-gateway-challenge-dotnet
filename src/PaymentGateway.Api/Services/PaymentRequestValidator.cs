using System.Collections.Frozen;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

using PaymentGateway.Api.Interfaces;
using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Services
{
    /// <inheritdoc />
    public class PaymentRequestValidator(IIsoCodeProvider isoCodeProvider) : IPaymentRequestValidator
    {
        /// <inheritdoc />
        public bool Validate(PostPaymentRequest request, out ValidatedPostPaymentRequest validatedPostPayment)
        {
            validatedPostPayment = new();
            if (!HasRequiredValues(request)) 
            {
                return false;
            }
                     
            if (!isoCodeProvider.IsAcceptedIsoCode(request.Currency))
            {
                return false;
            }
            validatedPostPayment.Currency = request.Currency;

            if (!CorrectCardLength((long)request.CardNumber))
            {
                return false;
            }
            validatedPostPayment.CardNumber = (long)request.CardNumber;

            if (!ValidExpiryDate((int)request.ExpiryMonth, (int)request.ExpiryYear, out int fourDigitYear))
            {
                return false;
            }
            validatedPostPayment.ExpiryMonth = (int)request.ExpiryMonth;
            validatedPostPayment.ExpiryYear = fourDigitYear;

            if (!ValidCvv((int)request.Cvv))
            {
                return false;
            }
            validatedPostPayment.Cvv = (int)request.Cvv;

            if (!ValidAmount((int)request.Amount))
            {
                return false;
            }

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

        /// <summary>
        /// Amount must be greater than zero
        /// </summary>
        /// <param name="amount">the request amount</param>
        /// <returns><see cref="bool"/></returns>
        private static bool ValidAmount(int amount)
        {
            return amount > 0;
        }

        /// <summary>
        /// Check request has all required values
        /// </summary>
        /// <param name="request">the <see cref="PostPaymentRequest"/></param>
        /// <returns><see cref="bool"/></returns>
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

        /// <summary>
        /// Ensure the expiry date is in the future and year is formatted correctly
        /// </summary>
        /// <param name="month"></param>
        /// <param name="year"></param>
        /// <param name="fourDigitYear"></param>
        /// <returns></returns>
        private static bool ValidExpiryDate(int month, int year, out int fourDigitYear)
        {
            fourDigitYear = year;
            if (month < 1 || month > 12)
            {
                return false;
            }

            int yearLength = (int)Math.Floor(Math.Log10(year) + 1);
            if (yearLength.Equals(2))
            {
                fourDigitYear = CultureInfo.CurrentCulture.Calendar.ToFourDigitYear(year);
            }
            else if (!yearLength.Equals(4))
            { 
                return false;
            }

            DateTime expiry = new DateTime(fourDigitYear, month, 28);

            if (expiry < DateTime.UtcNow)
            {
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// Ensure Cvv is 3 / 4 digits long
        /// </summary>
        /// <param name="cvv"></param>
        /// <returns></returns>
        private static bool ValidCvv(int cvv)
        {
            int cvvLength = (int)Math.Floor(Math.Log10(cvv) + 1);
            return (cvvLength.Equals(3) || cvvLength.Equals(4));

        }
    }
}

    /// <summary>
    /// Provides list of valid Iso codes
    /// </summary>
    public interface IIsoCodeProvider
    {
        /// <summary>
        /// Compares given Iso code against list of valid codes
        /// </summary>
        /// <param name="isoCode">the code to check</param>
        /// <returns><see cref="bool"/>true if found, false otherwise</returns>
        public bool IsAcceptedIsoCode(string isoCode);
    }


    /// <inheritdoc />
    public class IsoCodeValidator : IIsoCodeProvider
{
        private static readonly FrozenSet<string> _allowedIsoList;

        static IsoCodeValidator()
        {
            _allowedIsoList = new[] {
                "usd",
                "gbp",
                "eur"}.ToFrozenSet();
        }

        /// <inheritdoc />
        public bool IsAcceptedIsoCode(string isoCode)
        {
            return _allowedIsoList.Contains(isoCode, StringComparer.InvariantCultureIgnoreCase);
    }
}
