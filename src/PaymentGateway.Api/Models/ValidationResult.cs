namespace PaymentGateway.Api.Models
{
    public class ValidationResult
    {
        public bool Success { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}
