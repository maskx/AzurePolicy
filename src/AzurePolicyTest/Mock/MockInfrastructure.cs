using maskx.ARMOrchestration.ARMTemplate;
using maskx.ARMOrchestration.Orchestrations;
using maskx.AzurePolicy;
using maskx.AzurePolicy.Definitions;
using maskx.AzurePolicy.Services;
using System.Collections.Generic;
using System.Linq;

namespace AzurePolicyTest.Mock
{
    public class MockInfrastructure : IInfrastructure
    {
        private readonly Logical _Logical;
        public MockInfrastructure(Logical logical)
        {
            this._Logical = logical;
        }
        
        public List<(PolicyDefinition PolicyDefinition, string Parameter)> GetPolicyDefinitions(string scope, EvaluatingPhase evaluatingPhase)
        {
            var rtv = new List<(PolicyDefinition PolicyDefinition, string Parameter)>();
            var seg = scope.Split("/");
            var sub = seg[2];
            var rg = seg[^1].Replace('_', '/');
            rtv.Add((PolicyDefinition.Parse(TestHelper.GetJsonFileContent($"JSON/policy/{rg}")),
                string.Empty));
            if (rg == "Effect/Disabled")
                rtv.Add((PolicyDefinition.Parse(TestHelper.GetJsonFileContent("JSON/policy/effect/deny")),
                string.Empty));
            return rtv;
        }

        public List<(PolicyInitiative PolicyInitiative, string Parameter)> GetPolicyInitiatives(string scope, EvaluatingPhase evaluatingPhase)
        {
            var rtv = new List<(PolicyInitiative PolicyInitiative, string Parameter)>();
            return rtv;
        }

        public Template GetARMTemplateByScope(string scope)
        {
            return new Template();
        }

        public bool ResourceIsExisting(string type, string name, string resourceGroup, string scope, string condition, Dictionary<string, object> context)
        {
            if (name == "ExistsInInfrastructure")
                return true;
            if (name == "networkInterfaceName1")
                return true;
            
            // step 1: filter by type,name,resourceGrup,scope
            // setp 1: get from database or API
            List<Resource> resourcesGetByStep1 = new List<Resource>();
            
            // step 2: filter by condition
            if (!string.IsNullOrEmpty(condition))
            {
                var policyContext = context[maskx.AzurePolicy.Functions.ContextKeys.POLICY_CONTEXT] as PolicyContext;
  
                var policyNew = new PolicyContext()
                {
                    EvaluatingPhase=policyContext.EvaluatingPhase,
                    Parameters=policyContext.Parameters
                };
                // 因为 resource 的属性都是不用计算的值，因此不会用到DeploymentOrchestrationInput
                var deployNew = new DeploymentOrchestrationInput()
                {

                };
                return resourcesGetByStep1.Any((r) =>
                {
                    policyNew.Resource = r.ToString();
                    return this._Logical.Evaluate(policyNew, deployNew);
                });

            }
            return false;
        }

        public void Deploy(DeploymentOrchestrationInput input)
        {
            throw new System.NotImplementedException();
        }

        public bool Audit(string detail, Dictionary<string, object> context)
        {
            throw new System.NotImplementedException();
        }
    }
}
