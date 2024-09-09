namespace PaymentGateway.Api.Models.Responses
{
    public class PostPaymentResult
    {
        public bool IsSuccess { get; set; }
        public PostPaymentResponse PostPaymentResponse { get; set; }
        public string ErrorMessage { get; set; }

        public PostPaymentResult(bool isSuccess, PostPaymentResponse response, string? errorMessage = null)
        {
            IsSuccess = isSuccess;
            PostPaymentResponse = response;
            ErrorMessage = errorMessage;
        }
    }
}
