using maskx.ARMOrchestration.ARMTemplate;
using maskx.AzurePolicy;
using maskx.AzurePolicy.Definitions;
using maskx.AzurePolicy.Services;
using System.Collections.Generic;

namespace AzurePolicyTest.Mock
{
    public class MockInfrastructure : IInfrastructure
    {
        public BuiltinServiceType BuitinServiceTypes { get; set; } = new BuiltinServiceType();
        public BuiltinPathSegment BuiltinPathSegment { get; set; } = new BuiltinPathSegment();

        public List<(PolicyDefinition PolicyDefinition, string Parameter)> GetPolicyDefinitions(string scope, EvaluatingPhase evaluatingPhase)
        {
            var rtv = new List<(PolicyDefinition PolicyDefinition, string Parameter)>();
            var seg = scope.Split("/");
            var sub = seg[2];
            var rg = seg[^1];
            rtv.Add((PolicyDefinition.Parse(TestHelper.GetJsonFileContent($"JSON/policy/{rg}")),
                string.Empty));
            if (rg == "Effect_Disabled")
                rtv.Add((PolicyDefinition.Parse(TestHelper.GetJsonFileContent("JSON/policy/effect_deny")),
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
    }
}
