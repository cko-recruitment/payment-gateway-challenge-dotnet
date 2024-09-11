namespace PaymentGateway.Api.Models.Responses
{
    public class PostToBankResponseResult
    {
        public bool IsSuccess { get; set; }
        public PostToBankResponse PostToBankResponse { get; set; }
        public string ErrorMessage { get; set; }

        public PostToBankResponseResult(bool isSuccess, PostToBankResponse postToBankResponse, string? errorMessage = null)
        {
            IsSuccess = isSuccess;
            PostToBankResponse = postToBankResponse;
            ErrorMessage = string.IsNullOrEmpty(errorMessage) ? string.Empty : errorMessage;
        }
    }
}
