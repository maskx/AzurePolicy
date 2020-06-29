using maskx.AzurePolicy.Functions;
using maskx.AzurePolicy.Services;
using Microsoft.Extensions.DependencyInjection;

namespace maskx.AzurePolicy.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection UsingPolicyService(this IServiceCollection services)
        {
            services.AddSingleton<Effect>();
            services.AddSingleton<Condition>();
            services.AddSingleton<Logical>();
            services.AddSingleton<ARMOrchestration.Functions.ARMFunctions>();
            services.AddSingleton<PolicyFunction>();
            services.AddSingleton<PolicyService>();
            return services;
        }
    }
}
