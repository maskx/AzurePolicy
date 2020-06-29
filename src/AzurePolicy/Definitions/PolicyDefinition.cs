using maskx.AzurePolicy.Services;
using System.Collections.Generic;

namespace maskx.AzurePolicy.Definitions
{
    /// <summary>
    /// <see cref="https://docs.microsoft.com/en-us/azure/governance/policy/concepts/definition-structure"/>
    /// <seealso cref="https://docs.microsoft.com/en-us/azure/templates/microsoft.authorization/policyassignments#template-format"/>
    /// </summary>
    public class PolicyDefinition
    {
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Scope { get; set; }
        public List<string> NotScopes { get; set; }
        public PolicyTypeEnum Type { get; set; }
        public string Mode { get; set; }
        public string Metadata { get; set; }
        public string Parameters { get; set; }
        public PolicyRule PolicyRule { get; set; }
        internal int EffectPriority { get; set; }
        internal string EffectName { get; set; }
        internal string EffectDetail { get; set; }
    }
}
