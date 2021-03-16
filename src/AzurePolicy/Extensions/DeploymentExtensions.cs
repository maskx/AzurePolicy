using maskx.ARMOrchestration;
using maskx.AzurePolicy.Definitions;
using maskx.AzurePolicy.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace maskx.AzurePolicy.Extensions
{
    public static class DeploymentExtensions
    {
        public static List<(PolicyDefinition PolicyDefinition, string Parameter)> GetPolicyList(this Deployment deployment,IServiceProvider serviceProvider,EvaluatingPhase evaluatingPhase)
        {
            deployment.ServiceProvider = serviceProvider;
            var armInfra = serviceProvider.GetService<maskx.ARMOrchestration.IInfrastructure>();
            var policyInfra = serviceProvider.GetService<maskx.AzurePolicy.IInfrastructure>();
            var scope = "";
            if (!string.IsNullOrEmpty(deployment.SubscriptionId))
            {
                scope = $"/{armInfra.BuiltinPathSegment.Subscription}/{deployment.SubscriptionId}";
            }
            if (!string.IsNullOrEmpty(deployment.ManagementGroupId))
            {
                scope = $"/{armInfra.BuiltinPathSegment.ManagementGroup}/{deployment.ManagementGroupId}";
            }
            if (!string.IsNullOrEmpty(deployment.ResourceGroup))
                scope += $"/{armInfra.BuiltinPathSegment.ResourceGroup}/{deployment.ResourceGroup}";
            if (string.IsNullOrEmpty(scope))
                throw new ArgumentException("input not set scope");
            var policies = policyInfra.GetPolicyDefinitions(scope, evaluatingPhase);
            var initiatives = policyInfra.GetPolicyInitiatives(scope, evaluatingPhase);
            var policyFunc = serviceProvider.GetService<Functions.PolicyFunction>();
            foreach (var item in initiatives)
            {
                policies.AddRange(PolicyInitiative.ExpandePolicyDefinitions(item.PolicyInitiative, item.Parameter, policyFunc));
            }
            return policies;
        }
    }
}
