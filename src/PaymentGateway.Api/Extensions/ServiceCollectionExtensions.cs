using PaymentGateway.Api.Interfaces;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddPaymentServices(this IServiceCollection services) 
        {
            IIsoCodeProvider isoCodeProvider = new IsoCodeValidator();
            services.AddSingleton(isoCodeProvider);
            services.AddSingleton<IPaymentsHandler, PaymentsHandler>();
            services.AddSingleton<IPaymentsRepository, PaymentsRepository>();
            services.AddSingleton<IPaymentRequestValidator, PaymentRequestValidator>();
        }
    }
}
