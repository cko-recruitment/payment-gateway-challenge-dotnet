using System.Collections.Frozen;

using PaymentGateway.Api.Interfaces;
using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Services
{
    /// <inheritdoc />
    public class PaymentRequestValidator(IIsoCodeProvider isoCodeProvider) : IPaymentRequestValidator
    {
        /// <inheritdoc />
        public bool Validate(PostPaymentRequest request)
        {
            if (!isoCodeProvider.IsValidIsoCode(request.Currency))
            {
                return false;
            }
            return false;
        }
    }


    public interface IIsoCodeProvider
    {
        public bool IsValidIsoCode(string isoCode);
    }


    public class IsoCodeProvider : IIsoCodeProvider
    {
        private static readonly FrozenSet<string> _allowedIsoList;

        static IsoCodeProvider()
        {
            _allowedIsoList = new[] {
                "usd",
                "gbp",
                "eur"}.ToFrozenSet();
        }

        public bool IsValidIsoCode(string isoCode)
        {
            return _allowedIsoList.Contains(isoCode, StringComparer.InvariantCultureIgnoreCase);
        }
    }
}