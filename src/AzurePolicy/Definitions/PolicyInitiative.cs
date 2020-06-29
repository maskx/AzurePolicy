using maskx.AzurePolicy.Extensions;
using maskx.AzurePolicy.Functions;
using System.Collections.Generic;
using System.Text.Json;

namespace maskx.AzurePolicy.Definitions
{
    /// <summary>
    /// <see cref="https://docs.microsoft.com/en-us/azure/governance/policy/concepts/initiative-definition-structure"/>
    /// </summary>
    public class PolicyInitiative
    {
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public PolicyTypeEnum PolicyType { get; set; }
        public string Metadata { get; set; }
        public string Parameters { get; set; }
        public List<(PolicyDefinition PolicyDefinition,string Parameters)> PolicyDefinitions { get; set; }
        public List<PolicyDefinitionGroup> PolicyDefinitionGroups { get; set; }
        public static List<(PolicyDefinition PolicyDefinition, string Parameter)> ExpandePolicyDefinitions(PolicyInitiative initiative,string parameter,PolicyFunction function)
        {
            var input = MergeParameters(parameter, initiative.Parameters, function);
            var pds = new List<(PolicyDefinition PolicyDefinition, string Parameter)>();
            foreach (var item in initiative.PolicyDefinitions)
            {
                pds.Add((item.PolicyDefinition, MergeParameters(input,item.Parameters,function)));
            }
            return pds;
        }
        static string MergeParameters(string input, string define, PolicyFunction function)
        {
            Dictionary<string, object> context = new Dictionary<string, object>() {
                { ContextKeys.INITIATIVE_PARAMERTERS,input}
            };
            using var docDefine = JsonDocument.Parse(define);
            var rootDefine = docDefine.RootElement;
            return rootDefine.ExpandObject(context, function);
        }
    }
}
