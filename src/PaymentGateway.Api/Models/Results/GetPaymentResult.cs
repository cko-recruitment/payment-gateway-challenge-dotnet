using System.Net;

using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Models.Results
{
    public class GetPaymentResult
    {
        public bool IsSuccess { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public GetPaymentResponse GetPaymentResponse { get; set; }
        public string ErrorMessage { get; set; }

        public GetPaymentResult(GetPaymentResponse getPaymentResponse)
        {
            IsSuccess = true;
            StatusCode = HttpStatusCode.OK;
            GetPaymentResponse = getPaymentResponse;
        }

        public GetPaymentResult() { }
    }
}
