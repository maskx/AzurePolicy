using maskx.ARMOrchestration;
using maskx.AzurePolicy.Definitions;
using maskx.AzurePolicy.Extensions;
using maskx.AzurePolicy.Functions;
using maskx.OrchestrationService;
using System;
using System.Collections.Generic;
using System.Linq;

namespace maskx.AzurePolicy.Services
{
    public class PolicyService : IPolicyService
    {
        private readonly Logical _Logical;
        private readonly IInfrastructure _Infrastructure;
        private readonly ARMOrchestration.IInfrastructure _ARMInfrastructure;
        private readonly PolicyFunction _PolicyFunction;
        private readonly EffectService _Effect;
        private readonly IServiceProvider _ServiceProvider;

        public PolicyService(Logical logical,
            IInfrastructure infrastructure,
            PolicyFunction function,
            EffectService effect,
            ARMOrchestration.IInfrastructure aRMInfrastructure,
            IServiceProvider serviceProvider)
        {
            this._Infrastructure = infrastructure;
            this._Logical = logical;
            this._PolicyFunction = function;
            this._Effect = effect;
            this._ARMInfrastructure = aRMInfrastructure;
            this._ServiceProvider = serviceProvider;
        }

        #region Validate

        /// <summary>
        /// run at create resource phase
        /// </summary>
        public ValidationResult Validate(Deployment input)
        {
            return Validate(input, input.GetPolicyList(_ServiceProvider, EvaluatingPhase.Validation));
        }

        public ValidationResult Validate(Deployment input, List<(PolicyDefinition PolicyDefinition, string Parameter)> policyDefinitions)
        {
            ValidationResult validationResult = new ValidationResult() { Result = true };
            var (r, m) = input.Validate(_ServiceProvider);
            if (!r)
            {
                return new ValidationResult() { Result = false, Message = m };
            }
            if (policyDefinitions.Count == 0)
                return validationResult;
            var context = new Dictionary<string, object>();
            bool continueNext = true;
            PolicyContext policyContext = null;
            foreach (var policy in policyDefinitions.OrderBy((e) => { return e.PolicyDefinition.PolicyRule.Then.Priority; }))
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

        public void Remedy(Deployment input, List<(PolicyDefinition PolicyDefinition, string Parameter)> policyDefinitions)
        {
            var context = new Dictionary<string, object>();
            foreach (var policy in policyDefinitions.OrderBy((e) => { return e.PolicyDefinition.PolicyRule.Then.Priority; }))
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
        public void Audit(Deployment input, List<(PolicyDefinition PolicyDefinition, string Parameter)> policyDefinitions)
        {
            var context = new Dictionary<string, object>();
            foreach (var policy in policyDefinitions.OrderBy((e) => { return e.PolicyDefinition.PolicyRule.Then.Priority; }))
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
                        this._Effect.Run(pc);
                    }
                }
            }
            foreach (var deploy in input.EnumerateDeployments())
            {
                Audit(deploy, policyDefinitions);
            }
        }

        public TaskResult EvaluateResource(ARMOrchestration.ARMTemplate.Resource resource)
        {
            var policies = this._Infrastructure.GetPolicyDefinitions(resource.ResourceId, EvaluatingPhase.Auditing);
            var initiatives = this._Infrastructure.GetPolicyInitiatives(resource.ResourceId, EvaluatingPhase.Auditing);
            foreach (var item in initiatives)
            {
                policies.AddRange(PolicyInitiative.ExpandePolicyDefinitions(item.PolicyInitiative, item.Parameter, _PolicyFunction));
            }
            if (policies.Count == 0)
                return new TaskResult(200, "");
            return new TaskResult(200, "");
        }

        public TaskResult EvaluateDeployment(Deployment deployment)
        {
            var r = Validate(deployment, deployment.GetPolicyList(_ServiceProvider, EvaluatingPhase.Validation));
            if (r.Result)
                return new TaskResult(200, r.Message);
            return new TaskResult(400, r);
        }
        #endregion Audit
    }
}