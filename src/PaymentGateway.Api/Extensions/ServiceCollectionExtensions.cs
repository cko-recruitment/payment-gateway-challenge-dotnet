using PaymentGateway.Api.Interfaces;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddPaymentServices(this IServiceCollection services) 
        {
            IIsoCodeValidator isoValidator = new IsoCodeValidator();
            services.AddSingleton(isoValidator);
            services.AddSingleton<IPaymentsHandler, PaymentsHandler>();
            services.AddSingleton<IPaymentsRepository, PaymentsRepository>();
            services.AddSingleton<IPaymentRequestValidator, PaymentRequestValidator>();
        }
    }
}
