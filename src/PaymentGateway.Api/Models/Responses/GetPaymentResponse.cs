﻿using PaymentGateway.Api.Constants.Enums;

namespace PaymentGateway.Api.Models.Responses
{
    public class GetPaymentResponse
    {
        // defining duplicate class identical to Payment on purpose
        public Guid Id { get; set; }
        public PaymentStatus Status { get; set; }
        public string CardNumber { get; set; }
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
        public string Currency { get; set; }
        public int Amount { get; set; }
    }
}
