using PaymentGateway.Api.Interfaces;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Extensions
{
    /// <summary>
    /// Extension methods for the <see cref="IServiceCollection"/>
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Register all payment services agains the <see cref="IServiceCollection"/>
        /// </summary>
        /// <param name="services">An <see cref="IServiceCollection"/></param>
        public static void AddPaymentServices(this IServiceCollection services) 
        {
            // create an isoCodeProvider now due to its expensive frozen set initialization
            IIsoCodeProvider isoCodeProvider = new IsoCodeValidator();
            services.AddSingleton(isoCodeProvider);
            services.AddSingleton<IPaymentsHandler, PaymentsHandler>();
            services.AddSingleton<IPaymentsRepository, PaymentsRepository>();
            services.AddSingleton<IPaymentRequestValidator, PaymentRequestValidator>();
        }
    }
}
