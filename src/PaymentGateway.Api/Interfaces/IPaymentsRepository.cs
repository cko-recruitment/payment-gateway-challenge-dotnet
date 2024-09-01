using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Interfaces
{
    /// <summary>
    /// A Payments repository
    /// </summary>
    public interface IPaymentsRepository
    {
        /// <summary>
        /// Gets a PostPaymentResponse by Id
        /// </summary>
        /// <param name="id">Id of <see cref="PostPaymentResponse"/> to retrieve</param>
        /// <returns>A <see cref="PostPaymentResponse"/></returns>
        PostPaymentResponse Get(Guid id);

        /// <summary>
        /// Add a new <see cref="PostPaymentResponse"/> to the repository
        /// </summary>
        /// <param name="payment">The payment to add</param>
        public void Add(PostPaymentResponse payment);
    }
}
