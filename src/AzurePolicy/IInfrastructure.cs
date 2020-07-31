using maskx.ARMOrchestration.ARMTemplate;
using maskx.ARMOrchestration.Orchestrations;
using maskx.AzurePolicy.Definitions;
using maskx.AzurePolicy.Services;
using System.Collections.Generic;

namespace maskx.AzurePolicy
{
    public interface IInfrastructure
    {
        List<(PolicyDefinition PolicyDefinition,string Parameter)> GetPolicyDefinitions(string scope, EvaluatingPhase evaluatingPhase);
        List<(PolicyInitiative PolicyInitiative,string Parameter)> GetPolicyInitiatives(string scope, EvaluatingPhase evaluatingPhase);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="detail"></param>
        /// <param name="context"></param>
        /// <returns>when write audit success, should return true, when write fail should be false</returns>
        bool Audit(string detail,Dictionary<string, object> context);
        Template GetARMTemplateByScope(string scope);
        BuiltinServiceType BuitinServiceTypes { get; set; }
        BuiltinPathSegment BuiltinPathSegment { get; set; }
        bool ResourceIsExisting(string type, string name, string resourceGroup, string scope, string condition);
        void Deploy(DeploymentOrchestrationInput input);
    }
}
