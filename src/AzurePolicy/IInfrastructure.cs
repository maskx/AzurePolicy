using maskx.ARMOrchestration;
using maskx.ARMOrchestration.Orchestrations;
using maskx.AzurePolicy.Definitions;
using maskx.AzurePolicy.Services;
using System.Collections.Generic;

namespace maskx.AzurePolicy
{
    public interface IInfrastructure
    {
        List<(PolicyDefinition PolicyDefinition, string Parameter)> GetPolicyDefinitions(string scope, EvaluatingPhase evaluatingPhase);

        List<(PolicyInitiative PolicyInitiative, string Parameter)> GetPolicyInitiatives(string scope, EvaluatingPhase evaluatingPhase);

        /// <summary>
        ///
        /// </summary>
        /// <param name="message"></param>
        /// <param name="context"></param>
        /// <returns>when write audit success, should return true, when write fail should be false</returns>
        bool Audit(string message, Dictionary<string, object> context);

        Deployment GetDeploymentOrchestrationInput(string scope);

        bool ResourceIsExisting(string type, string name, string resourceGroup, string scope, string condition, Dictionary<string, object> context);

        void Deploy(Deployment input);
    }
}