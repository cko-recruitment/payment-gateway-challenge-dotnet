using System.ComponentModel.DataAnnotations;

using PaymentGateway.Api.Exceptions;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Services.Interfaces;

namespace PaymentGateway.Api.Validators;

public class RequestValidator : IRequestValidator
{
    private readonly IPaymentsRepository _paymentsRepository;
    private readonly PostPaymentRequestValidator _postPaymentRequestValidator;

    public RequestValidator(IPaymentsRepository paymentsRepository,
        PostPaymentRequestValidator postPaymentRequestValidator)
    {
        _paymentsRepository = paymentsRepository;
        _postPaymentRequestValidator = postPaymentRequestValidator;
    }

    public void Validate(PostPaymentRequest request, string idempotencyKey)
    {
        var errors = new Dictionary<string, string[]>();

        if (!Guid.TryParse(idempotencyKey, out Guid idempotencyGuid))
        {
            errors["Idempotency-Key"] = new[] { "Idempotency-Key must be a valid Guid." };
        }
        else if (_paymentsRepository.Exists(idempotencyGuid))
        {
            throw new DuplicatePaymentException(idempotencyKey);
        }

        var postPaymentValidationResult = _postPaymentRequestValidator.Validate(request);


        if (!postPaymentValidationResult.IsValid)
        {
            foreach (var failure in postPaymentValidationResult.Errors)
            {
                errors[failure.PropertyName] = errors.ContainsKey(failure.PropertyName)
                    ? errors[failure.PropertyName].Append(failure.ErrorMessage).ToArray()
                    : new[] { failure.ErrorMessage };
            }
        }

        if (errors.Any())
        {
            throw new PaymentValidationException(errors);
        }
    }
}

public interface IRequestValidator
{
    void Validate(PostPaymentRequest request, string idempotencyKey);
}
