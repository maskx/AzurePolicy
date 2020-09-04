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
        private readonly IServiceProvider _ServiceProvider;

        public PolicyService(Logical logical,
            IInfrastructure infrastructure,
            PolicyFunction function,
            ARMFunctions aRMFunctions,
            Effect effect,
            ARMOrchestration.IInfrastructure aRMInfrastructure,
            IServiceProvider serviceProvider)
        {
            this._Infrastructure = infrastructure;
            this._Logical = logical;
            this._PolicyFunction = function;
            this._ARMFunction = aRMFunctions;
            this._Effect = effect;
            this._ARMInfrastructure = aRMInfrastructure;
            this._ServiceProvider = serviceProvider;
        }

        #region Validate

        /// <summary>
        /// run at create resource phase
        /// </summary>
        public ValidationResult Validate(DeploymentOrchestrationInput input)
        {
            input.ServiceProvider = _ServiceProvider;
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
            if (string.IsNullOrEmpty(scope))
                throw new ArgumentException("input not set scope");
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
            ValidationResult validationResult = new ValidationResult() { Result = true, DeploymentOrchestrationInput = input };
            var (r, m) = input.Validate(_ServiceProvider);
            if (!r)
            {
                return new ValidationResult() { Result = false, Message = m, DeploymentOrchestrationInput = input };
            }
            if (policyDefinitions.Count == 0)
                return validationResult;
            var context = new Dictionary<string, object>();
            bool continueNext = true;
            PolicyContext policyContext = null;
            foreach (var policy in policyDefinitions.OrderBy((e) => { return _Effect.ParseEffect(e.PolicyDefinition, context); }))
            {
                foreach (var resource in input.EnumerateResource(true, true))
                {
                    if (resource.Type == _ARMInfrastructure.BuiltinServiceTypes.Deployments)
                        continue;
                    policyContext = new PolicyContext()
                    {
                        PolicyDefinition = policy.PolicyDefinition,
                        Parameters = policy.Parameter,
                        Resource = resource,
                        EvaluatingPhase = EvaluatingPhase.Validation
                    };
                    if (_Logical.Evaluate(policyContext))
                    {
                        continueNext = this._Effect.Run(policyContext);
                    }
                    if (!continueNext)
                        break;
                }
                if (!continueNext)
                    break;
            }
            if (continueNext)
            {
                foreach (var deploy in input.EnumerateDeployments())
                {
                    var vr = Validate(deploy, policyDefinitions);
                    if (!vr.Result)
                        return vr;
                }
            }
            validationResult.Result = continueNext;
            validationResult.PolicyContext = policyContext;
            return validationResult;
        }

        #endregion Validate

        #region Remedy

        /// <summary>
        /// a task to repair exist resourc
        /// </summary>
        public void Remedy(string scope)
        {
            var policies = this._Infrastructure.GetPolicyDefinitions(scope, EvaluatingPhase.Remediation);
            var initiatives = this._Infrastructure.GetPolicyInitiatives(scope, EvaluatingPhase.Remediation);
            foreach (var item in initiatives)
            {
                policies.AddRange(PolicyInitiative.ExpandePolicyDefinitions(item.PolicyInitiative, item.Parameter, _PolicyFunction));
            }
            if (policies.Count == 0)
                return;
            Remedy(this._Infrastructure.GetDeploymentOrchestrationInput(scope), policies);
        }

        public void Remedy(DeploymentOrchestrationInput input, List<(PolicyDefinition PolicyDefinition, string Parameter)> policyDefinitions)
        {
            var context = new Dictionary<string, object>();
            foreach (var policy in policyDefinitions.OrderBy((e) => { return _Effect.ParseEffect(e.PolicyDefinition, context); }))
            {
                foreach (var resource in input.EnumerateResource(true, true))
                {
                    var pc = new PolicyContext()
                    {
                        PolicyDefinition = policy.PolicyDefinition,
                        Parameters = policy.Parameter,
                        Resource = resource,
                        EvaluatingPhase = EvaluatingPhase.Remediation
                    };
                    if (_Logical.Evaluate(pc))
                    {
                        // TODO: 如果一个Remediation执行了，意味着资源发生了改变。
                        // 后续的policy应该在这个改变的基础上进行
                        // 因此，需要将 Effect 改变后的 Resource 存入 DeploymentOrchestrationInput中
                        // 这样后续的 Policy 才会 检测修正后的Resource

                        //this._Infrastructure.Audit(pc.PolicyDefinition.EffectDetail, new Dictionary<string, object>() {
                        //    {Functions.ContextKeys.POLICY_CONTEXT,pc },
                        //    {Functions.ContextKeys.DEPLOY_CONTEXT,input }
                        //});
                    }
                }
            }
        }

        #endregion Remedy

        #region Audit

        /// <summary>
        /// a task to generate a compliance report
        /// </summary>
        public void Audit(string scope)
        {
            // deny effect should report an non-compliant
            // https://docs.microsoft.com/en-us/azure/governance/policy/concepts/effects#layering-policy-definitions
            var policies = this._Infrastructure.GetPolicyDefinitions(scope, EvaluatingPhase.Auditing);
            var initiatives = this._Infrastructure.GetPolicyInitiatives(scope, EvaluatingPhase.Auditing);
            foreach (var item in initiatives)
            {
                policies.AddRange(PolicyInitiative.ExpandePolicyDefinitions(item.PolicyInitiative, item.Parameter, _PolicyFunction));
            }
            if (policies.Count == 0)
                return;
            Audit(this._Infrastructure.GetDeploymentOrchestrationInput(scope), policies);
        }
        public void Audit(DeploymentOrchestrationInput input, List<(PolicyDefinition PolicyDefinition, string Parameter)> policyDefinitions)
        {
            var context = new Dictionary<string, object>();
            foreach (var policy in policyDefinitions.OrderBy((e) => { return _Effect.ParseEffect(e.PolicyDefinition, context); }))
            {
                foreach (var resource in input.EnumerateResource(true, true))
                {
                    var pc = new PolicyContext()
                    {
                        PolicyDefinition = policy.PolicyDefinition,
                        Parameters = policy.Parameter,
                        Resource = resource,
                        EvaluatingPhase = EvaluatingPhase.Auditing
                    };
                    if (_Logical.Evaluate(pc))
                    {
                        this._Infrastructure.Audit(pc.PolicyDefinition.EffectDetail, new Dictionary<string, object>() {
                            {Functions.ContextKeys.POLICY_CONTEXT,pc }
                        });
                    }
                }
            }
            foreach (var deploy in input.EnumerateDeployments())
            {
                Audit(deploy, policyDefinitions);
            }
        }
        #endregion Audit
    }
}