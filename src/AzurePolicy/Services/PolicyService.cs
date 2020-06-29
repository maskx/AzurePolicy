using maskx.ARMOrchestration.Functions;
using maskx.ARMOrchestration.Orchestrations;
using maskx.AzurePolicy.Definitions;
using maskx.AzurePolicy.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace maskx.AzurePolicy.Services
{
    public class PolicyService
    {
        private readonly Logical _Logical;
        private readonly IInfrastructure _Infrastructure;
        private readonly PolicyFunction _PolicyFunction;
        private readonly ARMFunctions _ARMFunction;
        private readonly Effect _Effect;
        public PolicyService(Logical logical, IInfrastructure infrastructure, PolicyFunction function, ARMFunctions aRMFunctions, Effect effect)
        {
            this._Infrastructure = infrastructure;
            this._Logical = logical;
            this._PolicyFunction = function;
            this._ARMFunction = aRMFunctions;
            this._Effect = effect;
        }
        /// <summary>
        /// run at create resource phase
        /// </summary>
        public ValidationResult Validate(DeploymentContext deploymentContext)
        {
            var scope = "";
            if (!string.IsNullOrEmpty(deploymentContext.SubscriptionId))
            {
                scope = $"/{_Infrastructure.BuiltinPathSegment.Subscription}/{deploymentContext.SubscriptionId}";
            }
            if (!string.IsNullOrEmpty(deploymentContext.ManagementGroupId))
            {
                scope = $"/{_Infrastructure.BuiltinPathSegment.ManagementGroup}/{deploymentContext.ManagementGroupId}";
            }
            if (!string.IsNullOrEmpty(deploymentContext.ResourceGroup))
                scope += $"/{_Infrastructure.BuiltinPathSegment.ResourceGroup}/{deploymentContext.ResourceGroup}";
            var d = this._Infrastructure.GetPolicyDefinitions(scope, EvaluatingPhase.Validation);
            var i = this._Infrastructure.GetPolicyInitiatives(scope, EvaluatingPhase.Validation);
            foreach (var item in i)
            {
                d.AddRange(PolicyInitiative.ExpandePolicyDefinitions(item.PolicyInitiative, item.Parameter, _PolicyFunction));
            }
            using var doc = JsonDocument.Parse(deploymentContext.TemplateContent);
            var deniedPolicy = new List<PolicyDefinition>();
            foreach (var policy in d.OrderBy((e) => { return 1; }))
            {
                if (policy.PolicyDefinition.EffectName == Effect.DisabledEffectName)
                    return new ValidationResult() { Result = true, Message = "meet disabled policy", Template = deploymentContext.TemplateContent };
                foreach (var resource in doc.RootElement.GetProperty("resources").EnumerateArray())
                {
                    Validate(policy.PolicyDefinition, policy.Parameter, resource, deploymentContext, string.Empty);
                }
            }
            return new ValidationResult()
            {
                Result = deniedPolicy.Count <= 0,
                Template = deploymentContext.TemplateContent,
                DeniedPolicy = deniedPolicy
            };

        }
        private List<PolicyDefinition> Validate(PolicyDefinition policyDefinition, string parameter, JsonElement resource, DeploymentContext deploymentContext, string namePath)
        {
            var deniedPolicyList = new List<PolicyDefinition>();
            if (resource.TryGetProperty("resources", out JsonElement resources))
            {
                string n = namePath + "/" + this._ARMFunction.Evaluate(resource.GetProperty("name").GetString(),
                    new Dictionary<string, object>() {
                        {ARMOrchestration.Functions.ContextKeys.ARM_CONTEXT,deploymentContext }
                    });
                foreach (var r in resources.EnumerateArray())
                {
                    var deniedPolicy = Validate(policyDefinition, parameter, r, deploymentContext, n);
                    deniedPolicyList.AddRange(deniedPolicy);
                }
            }
            if (_Logical.Evaluate(new PolicyContext()
            {
                PolicyDefinition = policyDefinition,
                Parameters = parameter,
                Resource = resource.GetRawText(),
                NamePath = namePath
            }, deploymentContext))
            {
                if (string.Equals(policyDefinition.EffectName, Effect.DenyEffectName, StringComparison.OrdinalIgnoreCase))
                    deniedPolicyList.Add(policyDefinition);
                else
                {
                    deploymentContext.TemplateContent = this._Effect.Run(policyDefinition.EffectName, policyDefinition.EffectDetail, new Dictionary<string, object>() { });
                }
            }
            return deniedPolicyList;

        }
        /// <summary>
        /// a task to repair exist resourc
        /// </summary>
        public void Remedy(string scope)
        {

        }
        /// <summary>
        /// a task to generate a compliance report
        /// </summary>
        public void Audit(string scope)
        {

        }

    }
}
