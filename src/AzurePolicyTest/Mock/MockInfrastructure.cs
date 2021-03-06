﻿using maskx.ARMOrchestration;
using maskx.ARMOrchestration.ARMTemplate;
using maskx.AzurePolicy;
using maskx.AzurePolicy.Definitions;
using maskx.AzurePolicy.Services;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace AzurePolicyTest.Mock
{
    public class MockInfrastructure : maskx.AzurePolicy.IInfrastructure
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
            var rg = seg[^1].Replace('_', '/');

            rtv.Add((GetPolicyDefinition(rg), string.Empty));
            if (rg == "Effect/Disabled")
            {
                rtv.Add((GetPolicyDefinition("effect/deny"), string.Empty));
            }


            return rtv;
        }
        private PolicyDefinition GetPolicyDefinition(string name)
        {
            using var doc = JsonDocument.Parse(TestHelper.GetJsonFileContent($"JSON/policy/{name}"));
            return doc.RootElement.GetProperty("properties").GetRawText();
        }
        public List<(PolicyInitiative PolicyInitiative, string Parameter)> GetPolicyInitiatives(string scope, EvaluatingPhase evaluatingPhase)
        {
            var rtv = new List<(PolicyInitiative PolicyInitiative, string Parameter)>();
            return rtv;
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
                    EvaluatingPhase = policyContext.EvaluatingPhase,
                    Parameters = policyContext.Parameters
                };
                return resourcesGetByStep1.Any((r) =>
                {
                    policyNew.Resource = r;
                    return this._Logical.Evaluate(policyNew);
                });
            }
            return false;
        }

        public void Deploy(Deployment input)
        {
            throw new System.NotImplementedException();
        }

        public bool Audit(string detail, Dictionary<string, object> context)
        {
            throw new System.NotImplementedException();
        }

        public Deployment GetDeploymentOrchestrationInput(string scope)
        {
            throw new System.NotImplementedException();
        }
    }
}