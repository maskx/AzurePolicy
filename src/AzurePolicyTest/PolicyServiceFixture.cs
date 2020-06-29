using AzurePolicyTest.Mock;
using maskx.ARMOrchestration.Orchestrations;
using maskx.AzurePolicy;
using maskx.AzurePolicy.Extensions;
using maskx.AzurePolicy.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Xunit;

namespace AzurePolicyTest
{
    public class PolicyServiceFixture
    {
        public PolicyService PolicyService { get; private set; }
        public IServiceProvider ServiceProvider { get; private set; }
        public PolicyServiceFixture()
        {
            var workerHost = Host.CreateDefaultBuilder()
             .ConfigureAppConfiguration((hostingContext, config) =>
             {
                 config
                 .AddJsonFile("appsettings.json", optional: true)
                 .AddUserSecrets("A2865D9C-44FA-42CD-A9F6-278CDD99B107");
             })
             .ConfigureServices((hostContext, services) =>
             {
                 services.AddSingleton<maskx.ARMOrchestration.IInfrastructure, MockARMInfrastructure>();
                 services.AddSingleton<IInfrastructure,MockInfrastructure>();
                 services.UsingPolicyService();
             }).Build();
            workerHost.RunAsync();
            this.ServiceProvider = workerHost.Services;
            this.PolicyService = this.ServiceProvider.GetService<PolicyService>();
            var effect= this.ServiceProvider.GetService<Effect>();
            effect.SetEffect("ForTesting", (detail, context) =>
            {
                return (context[maskx.ARMOrchestration.Functions.ContextKeys.ARM_CONTEXT] as DeploymentContext).TemplateContent;
            });
        }
    }
    [CollectionDefinition("WebHost PolicyService")]
    public class WebHostCollection : ICollectionFixture<PolicyServiceFixture>
    {
    }
}
