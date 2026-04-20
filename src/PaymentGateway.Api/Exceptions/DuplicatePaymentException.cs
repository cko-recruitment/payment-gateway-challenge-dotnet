namespace PaymentGateway.Api.Exceptions
{
    public class DuplicatePaymentException : Exception
    {
        public DuplicatePaymentException(string id) : base($"Payment with idempotency key {id} already exists")
        {
        }
    }
}
