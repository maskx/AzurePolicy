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
        #region Validate

        /// <summary>
        /// run at create resource phase
        /// </summary>
        public ValidationResult Validate(DeploymentOrchestrationInput input)
        {
            var scope = "";
            if (!string.IsNullOrEmpty(input.SubscriptionId))
            {
                scope = $"/{_ARMInfrastructure.BuiltinPathSegment.Subscription}/{input.SubscriptionId}";
            }
            if (!string.IsNullOrEmpty(input.ManagementGroupId))
            {
                scope = $"/{_ARMInfrastructure.BuiltinPathSegment.ManagementGroup}/{input.ManagementGroupId}";
            }
            if (!string.IsNullOrEmpty(input.ResourceGroup))
                scope += $"/{_ARMInfrastructure.BuiltinPathSegment.ResourceGroup}/{input.ResourceGroup}";

            var policies = this._Infrastructure.GetPolicyDefinitions(scope, EvaluatingPhase.Validation);
            var initiatives = this._Infrastructure.GetPolicyInitiatives(scope, EvaluatingPhase.Validation);
            foreach (var item in initiatives)
            {
                policies.AddRange(PolicyInitiative.ExpandePolicyDefinitions(item.PolicyInitiative, item.Parameter, _PolicyFunction));
            }
            return Validate(input, policies);

        }

        public ValidationResult Validate(DeploymentOrchestrationInput input, List<(PolicyDefinition PolicyDefinition, string Parameter)> policyDefinitions)
        {
            ValidationResult validationResult = new ValidationResult() { Result = true };
            try
            {
                validationResult.DeploymentOrchestrationInput = DeploymentOrchestrationInput.Validate(input, _ARMFunction, _ARMInfrastructure);
            }
            catch (Exception ex)
            {
                validationResult.Result = false;
                validationResult.Message = ex.Message;
            }
            if (!validationResult.Result)
                return validationResult;
            if (policyDefinitions.Count == 0)
                return validationResult;
            using var doc = JsonDocument.Parse(input.TemplateContent);
            var context = new Dictionary<string, object>();
            bool continueNext = true;
            PolicyContext policyContext = null;
            foreach (var policy in policyDefinitions.OrderBy((e) => { return _Effect.ParseEffect(e.PolicyDefinition, context); }))
            {
                foreach (var resource in doc.RootElement.GetProperty("resources").EnumerateArray())
                {
                    (continueNext, policyContext) = Validate(new PolicyContext()
                    {
                        PolicyDefinition = policy.PolicyDefinition,
                        Parameters = policy.Parameter,
                        Resource = resource.GetRawText(),
                        EvaluatingPhase = EvaluatingPhase.Validation,
                        RootInput=input
                    }, input);
                    if (!continueNext)
                        break;
                }
                if (!continueNext)
                    break;
            }
            return new ValidationResult()
            {
                Result = continueNext,
                DeploymentOrchestrationInput = input,
                PolicyContext = policyContext
            };
        }
        private (bool ContinueNext, PolicyContext Policy) Validate(DeploymentOrchestrationInput root,DeploymentOrchestrationInput input, PolicyDefinition policyDefinition, string parameters)
        {
            using var doc = JsonDocument.Parse(input.TemplateContent);
            var context = new Dictionary<string, object>();
            bool continueNext = true;
            PolicyContext policyContext = null;
            foreach (var resource in doc.RootElement.GetProperty("resources").EnumerateArray())
            {
                policyContext = new PolicyContext()
                {
                    PolicyDefinition = policyDefinition,
                    Parameters = parameters,
                    Resource = resource.GetRawText(),
                    EvaluatingPhase = EvaluatingPhase.Validation,
                    RootInput=root
                };
                (continueNext, policyContext) = Validate(policyContext, input);
                if (!continueNext)
                    break;
            }
            return (continueNext, policyContext);
        }
        private (bool ContinueNext, PolicyContext Policy) Validate(PolicyContext policyContext, DeploymentOrchestrationInput input)
        {
            using var doc = JsonDocument.Parse(policyContext.Resource);
            var resource = doc.RootElement;
            string t = resource.GetProperty("type").GetString();

            // nest deployment
            if (t.Equals(_ARMInfrastructure.BuiltinServiceTypes.Deployments, StringComparison.OrdinalIgnoreCase))
            {
                string n = this._ARMFunction.Evaluate(resource.GetProperty("name").GetString(),
                   new Dictionary<string, object>() {
                        {ARMOrchestration.Functions.ContextKeys.ARM_CONTEXT,input }
                   }).ToString();
                var (ContinueNext, Policy) = Validate(policyContext.RootInput,input.Deployments[n],policyContext.PolicyDefinition,policyContext.Parameters);
                if (!ContinueNext)
                    return (false, Policy);
            }
            else if (resource.TryGetProperty("resources", out JsonElement resources))
            {
                string n = this._ARMFunction.Evaluate(resource.GetProperty("name").GetString(),
                    new Dictionary<string, object>() {
                        {ARMOrchestration.Functions.ContextKeys.ARM_CONTEXT,input }
                    }).ToString();
                if (!string.IsNullOrEmpty(policyContext.NamePath))
                {
                    n = policyContext.NamePath + "/" + n;
                    t = policyContext.ParentType + "/" + t;
                }
                foreach (var r in resources.EnumerateArray())
                {
                    var policyCxt = new PolicyContext()
                    {
                        NamePath = n,
                        ParentType = t,
                        Parameters = policyContext.Parameters,
                        PolicyDefinition = policyContext.PolicyDefinition,
                        Resource = r.GetRawText(),
                        RootInput=policyContext.RootInput
                    };
                    var (ContinueNext, Policy) = Validate(policyCxt, input);
                    if (!ContinueNext)
                        return (false, Policy);
                }
            }
            if (_Logical.Evaluate(policyContext, input))
            {
                return (this._Effect.Run(policyContext, input), policyContext);
            }
            return (true, null);
        }

        #endregion

        #region Remedy

        /// <summary>
        /// a task to repair exist resourc
        /// </summary>
        public void Remedy(string scope)
        {
            // question? deny effect should remove the resource?
        }

        #endregion

        #region Audit
        /// <summary>
        /// a task to generate a compliance report
        /// </summary>
        public void Audit(string scope)
        {
            // deny effect should report an non-compliant
            // https://docs.microsoft.com/en-us/azure/governance/policy/concepts/effects#layering-policy-definitions
            var d = this._Infrastructure.GetPolicyDefinitions(scope, EvaluatingPhase.Auditing);
            var i = this._Infrastructure.GetPolicyInitiatives(scope, EvaluatingPhase.Auditing);
            foreach (var item in i)
            {
                d.AddRange(PolicyInitiative.ExpandePolicyDefinitions(item.PolicyInitiative, item.Parameter, _PolicyFunction));
            }
            if (d.Count == 0)
                return;
            Audit(scope, d);
        }

        public void Audit(string scope, List<(PolicyDefinition PolicyDefinition, string Parameter)> policyDefinitions)
        {
            var template = this._Infrastructure.GetARMTemplateByScope(scope);
            DeploymentOrchestrationInput input = new DeploymentOrchestrationInput()
            {
                Template = template,
                TemplateContent = template.ToString()
            };
            using var doc = JsonDocument.Parse(input.TemplateContent);
            var deniedPolicy = new List<PolicyDefinition>();
            var context = new Dictionary<string, object>();
            foreach (var policy in policyDefinitions.OrderBy((e) => { return _Effect.ParseEffect(e.PolicyDefinition, context); }))
            {
                if (string.Equals(policy.PolicyDefinition.EffectName, Effect.DisabledEffectName, StringComparison.OrdinalIgnoreCase))
                    continue;
                foreach (var resource in doc.RootElement.GetProperty("resources").EnumerateArray())
                {
                    Audit(new PolicyContext()
                    {
                        PolicyDefinition = policy.PolicyDefinition,
                        Parameters = policy.Parameter,
                        Resource = resource.GetRawText(),
                        EvaluatingPhase = EvaluatingPhase.Auditing
                    }, input);
                }
            }
        }

        private bool Audit(PolicyContext policyContext, DeploymentOrchestrationInput deploymentContext)
        {
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
                    var policyCxt = new PolicyContext()
                    {
                        NamePath = n,
                        Parameters = policyContext.Parameters,
                        PolicyDefinition = policyContext.PolicyDefinition,
                        Resource = r.GetRawText()
                    };
                    Audit(policyCxt, deploymentContext);
                }
            }
            if (_Logical.Evaluate(policyContext, deploymentContext))
            {
                if (string.Equals(policyContext.PolicyDefinition.EffectName, Effect.DisabledEffectName, StringComparison.OrdinalIgnoreCase))
                    return false;
                return this._Effect.Run(policyContext, deploymentContext);
            }
            return true;
        }
        #endregion
    }
}
