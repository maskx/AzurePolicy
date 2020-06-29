using maskx.ARMOrchestration.ARMTemplate;
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
        public static Template Parse(string content)
        {
            Template template = new Template();
            using JsonDocument doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            if (root.TryGetProperty("$schema", out JsonElement schema))
                template.Schema = schema.GetString();
            if (root.TryGetProperty("contentVersion", out JsonElement contentVersion))
                template.ContentVersion = contentVersion.GetString();
            if (template.Schema.EndsWith("deploymentTemplate.json#", StringComparison.InvariantCultureIgnoreCase))
                template.DeployLevel = DeployLevel.ResourceGroup;
            else if (template.Schema.EndsWith("subscriptionDeploymentTemplate.json#", StringComparison.InvariantCultureIgnoreCase))
                template.DeployLevel = DeployLevel.Subscription;
            else if (template.Schema.EndsWith("managementGroupDeploymentTemplate.json#", StringComparison.InvariantCultureIgnoreCase))
                template.DeployLevel = DeployLevel.ManagemnetGroup;
            if (root.TryGetProperty("apiProfile", out JsonElement apiProfile))
                template.ApiProfile = apiProfile.GetString();
            if (root.TryGetProperty("parameters", out JsonElement parameters))
                template.Parameters = parameters.GetRawText();
            if (root.TryGetProperty("outputs", out JsonElement outputs))
                template.Outputs = outputs.GetRawText();
            return template;
        }
        /// <summary>
        /// run at create resource phase
        /// </summary>
        public ValidationResult Validate(DeploymentContext deploymentContext)
        {
            // TODO: replace this with ARMOrchestration's function
            deploymentContext.Template = Parse(deploymentContext.TemplateContent);

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
            var context = new Dictionary<string, object>();
            foreach (var policy in d.OrderBy((e) => { return _Effect.ParseEffect(e.PolicyDefinition, context); }))
            {
                if (string.Equals(policy.PolicyDefinition.EffectName, Effect.DisabledEffectName, StringComparison.OrdinalIgnoreCase))
                    continue;
                foreach (var resource in doc.RootElement.GetProperty("resources").EnumerateArray())
                {
                    deniedPolicy.AddRange(Validate(policy.PolicyDefinition, policy.Parameter, resource, deploymentContext, string.Empty));
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
