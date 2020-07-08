using maskx.ARMOrchestration.ARMTemplate;
using maskx.ARMOrchestration.Functions;
using maskx.ARMOrchestration.Orchestrations;
using maskx.AzurePolicy.Definitions;
using maskx.AzurePolicy.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Runtime.Versioning;
using System.Text.Json;

namespace maskx.AzurePolicy.Services
{
    public class PolicyService
    {
        private readonly Logical _Logical;
        private readonly IInfrastructure _Infrastructure;
        private readonly ARMOrchestration.IInfrastructure _ARMInfrastructure;
        private readonly PolicyFunction _PolicyFunction;
        private readonly ARMFunctions _ARMFunction;
        private readonly Effect _Effect;
        public PolicyService(Logical logical,
            IInfrastructure infrastructure,
            PolicyFunction function,
            ARMFunctions aRMFunctions,
            Effect effect,
            ARMOrchestration.IInfrastructure aRMInfrastructure)
        {
            this._Infrastructure = infrastructure;
            this._Logical = logical;
            this._PolicyFunction = function;
            this._ARMFunction = aRMFunctions;
            this._Effect = effect;
            this._ARMInfrastructure = aRMInfrastructure;
        }

        /// <summary>
        /// run at create resource phase
        /// </summary>
        public ValidationResult Validate(DeploymentOrchestrationInput input)
        {
            input.Template = Template.Parse(input.TemplateContent, input, _ARMFunction, _ARMInfrastructure);

            var scope = "";
            if (!string.IsNullOrEmpty(input.SubscriptionId))
            {
                scope = $"/{_Infrastructure.BuiltinPathSegment.Subscription}/{input.SubscriptionId}";
            }
            if (!string.IsNullOrEmpty(input.ManagementGroupId))
            {
                scope = $"/{_Infrastructure.BuiltinPathSegment.ManagementGroup}/{input.ManagementGroupId}";
            }
            if (!string.IsNullOrEmpty(input.ResourceGroup))
                scope += $"/{_Infrastructure.BuiltinPathSegment.ResourceGroup}/{input.ResourceGroup}";
            var d = this._Infrastructure.GetPolicyDefinitions(scope, EvaluatingPhase.Validation);
            var i = this._Infrastructure.GetPolicyInitiatives(scope, EvaluatingPhase.Validation);
            foreach (var item in i)
            {
                d.AddRange(PolicyInitiative.ExpandePolicyDefinitions(item.PolicyInitiative, item.Parameter, _PolicyFunction));
            }
            using var doc = JsonDocument.Parse(input.TemplateContent);
            var deniedPolicy = new List<PolicyDefinition>();
            var context = new Dictionary<string, object>();
            foreach (var policy in d.OrderBy((e) => { return _Effect.ParseEffect(e.PolicyDefinition, context); }))
            {
                if (string.Equals(policy.PolicyDefinition.EffectName, Effect.DisabledEffectName, StringComparison.OrdinalIgnoreCase))
                    continue;
                foreach (var resource in doc.RootElement.GetProperty("resources").EnumerateArray())
                {
                    deniedPolicy.AddRange(Validate(new PolicyContext()
                    {
                        PolicyDefinition = policy.PolicyDefinition,
                        Parameters = policy.Parameter,
                        Resource = resource.GetRawText()
                    }, input));
                }
            }
            return new ValidationResult()
            {
                Result = deniedPolicy.Count <= 0,
                Template = input.TemplateContent,
                DeniedPolicy = deniedPolicy
            };

        }
        private List<PolicyDefinition> Validate(PolicyContext policyContext, DeploymentOrchestrationInput deploymentContext)
        {
            var deniedPolicyList = new List<PolicyDefinition>();
            using var doc = JsonDocument.Parse(policyContext.Resource);
            var resource = doc.RootElement;
            if (resource.TryGetProperty("resources", out JsonElement resources))
            {
                string n = this._ARMFunction.Evaluate(resource.GetProperty("name").GetString(),
                    new Dictionary<string, object>() {
                        {ARMOrchestration.Functions.ContextKeys.ARM_CONTEXT,deploymentContext }
                    }).ToString();
                if (!string.IsNullOrEmpty(policyContext.NamePath))
                    n = policyContext.NamePath + "/" + n;
                foreach (var r in resources.EnumerateArray())
                {
                    var deniedPolicy = Validate(new PolicyContext()
                    {
                        NamePath = n,
                        Parameters = policyContext.Parameters,
                        PolicyDefinition = policyContext.PolicyDefinition,
                        Resource = r.GetRawText()
                    }, deploymentContext);
                    deniedPolicyList.AddRange(deniedPolicy);
                }
            }
            if (_Logical.Evaluate(policyContext, deploymentContext))
            {
                if (string.Equals(policyContext.PolicyDefinition.EffectName, Effect.DenyEffectName, StringComparison.OrdinalIgnoreCase))
                    deniedPolicyList.Add(policyContext.PolicyDefinition);
                else
                {
                    this._Effect.Run(policyContext, deploymentContext);
                }
            }
            return deniedPolicyList;

        }
        /// <summary>
        /// a task to repair exist resourc
        /// </summary>
        public void Remedy(string scope)
        {
            // question? deny effect should remove the resource?
        }
        /// <summary>
        /// a task to generate a compliance report
        /// </summary>
        public void Audit(string scope)
        {
            // deny effect should report an non-compliant
            // https://docs.microsoft.com/en-us/azure/governance/policy/concepts/effects#layering-policy-definitions
            var d = this._Infrastructure.GetPolicyDefinitions(scope, EvaluatingPhase.Validation);
            var i = this._Infrastructure.GetPolicyInitiatives(scope, EvaluatingPhase.Validation);
            foreach (var item in i)
            {
                d.AddRange(PolicyInitiative.ExpandePolicyDefinitions(item.PolicyInitiative, item.Parameter, _PolicyFunction));
            }
            if (d.Count == 0)
                return;
            var template = this._Infrastructure.GetARMTemplateByScope(scope);
            DeploymentContext deploymentContext = new DeploymentContext()
            {
                Template = template,
                TemplateContent = template.ToString()
            };
            using var doc = JsonDocument.Parse(deploymentContext.TemplateContent);
            var deniedPolicy = new List<PolicyDefinition>();
            var context = new Dictionary<string, object>();
            foreach (var policy in d.OrderBy((e) => { return _Effect.ParseEffect(e.PolicyDefinition, context); }))
            {
                if (string.Equals(policy.PolicyDefinition.EffectName, Effect.DisabledEffectName, StringComparison.OrdinalIgnoreCase))
                    continue;
                foreach (var resource in doc.RootElement.GetProperty("resources").EnumerateArray())
                {

                }
            }
        }
        public void Audit(string scope, PolicyDefinition policyDefinition)
        {

        }

    }
}
