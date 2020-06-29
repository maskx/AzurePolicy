using maskx.AzurePolicy.Definitions;
using maskx.AzurePolicy.Services;
using System.Collections.Generic;

namespace maskx.AzurePolicy
{
    public interface IInfrastructure
    {
        List<(PolicyDefinition PolicyDefinition,string Parameter)> GetPolicyDefinitions(string scope, EvaluatingPhase evaluatingPhase);
        List<(PolicyInitiative PolicyInitiative,string Parameter)> GetPolicyInitiatives(string scope, EvaluatingPhase evaluatingPhase);
        BuiltinServiceType BuitinServiceTypes { get; set; }
        BuiltinPathSegment BuiltinPathSegment { get; set; }
    }
}
