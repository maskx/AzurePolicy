using maskx.AzurePolicy.Functions;
using maskx.AzurePolicy.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace maskx.AzurePolicy.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection UsingPolicyService(this IServiceCollection services)
        {
            services.AddSingleton<Effect>();
            services.AddSingleton<Condition>();
            services.AddSingleton<Logical>();
            services.TryAddSingleton<ARMOrchestration.Functions.ARMFunctions>();
            services.AddSingleton<PolicyFunction>();
            services.AddSingleton<PolicyService>();
            return services;
        }
    }
}
