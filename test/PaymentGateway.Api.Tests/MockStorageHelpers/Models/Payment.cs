using PaymentGateway.Api.Constants.Enums;

namespace PaymentGateway.Api.Tests.MockStorageHelpers.Models
{
    // defining duplicate separate class as this shouldnt exist
    public class Payment
    {
        public Guid Id { get; set; }
        public PaymentStatus Status { get; set; }
        public string CardNumber { get; set; }
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
        public string Currency { get; set; }
        public int Amount { get; set; }
    }
}
