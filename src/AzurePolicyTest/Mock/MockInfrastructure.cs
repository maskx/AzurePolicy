using maskx.AzurePolicy;
using maskx.AzurePolicy.Definitions;
using maskx.AzurePolicy.Services;
using System.Collections.Generic;

namespace AzurePolicyTest.Mock
{
    public class MockInfrastructure : IInfrastructure
    {
        public BuiltinServiceType BuitinServiceTypes { get; set; }
        public BuiltinPathSegment BuiltinPathSegment { get; set; }

        public List<(PolicyDefinition PolicyDefinition, string Parameter)> GetPolicyDefinitions(string scope, EvaluatingPhase evaluatingPhase)
        {
            var rtv = new List<(PolicyDefinition PolicyDefinition, string Parameter)>();
            return rtv;
        }

        public List<(PolicyInitiative PolicyInitiative, string Parameter)> GetPolicyInitiatives(string scope, EvaluatingPhase evaluatingPhase)
        {
            var rtv = new List<(PolicyInitiative PolicyInitiative, string Parameter)>();
            return rtv;
        }
    }
}
