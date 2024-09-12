namespace PaymentGateway.Api.Models.Responses
{
    public class PostToBankResult
    {
        public bool IsSuccess { get; set; }
        public PostToBankResponse PostToBankResponse { get; set; }
        public string ErrorMessage { get; set; }

        public PostToBankResult(bool isSuccess, PostToBankResponse postToBankResponse, string? errorMessage = null)
        {
            IsSuccess = isSuccess;
            PostToBankResponse = postToBankResponse;
            ErrorMessage = string.IsNullOrEmpty(errorMessage) ? string.Empty : errorMessage;
        }
    }
}
