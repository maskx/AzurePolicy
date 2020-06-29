using System;
using System.Collections.Generic;
using System.Text.Json;

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
        public PolicyRule PolicyRule { get; set; } = new PolicyRule();
        internal int EffectPriority { get; set; }
        internal string EffectName { get; set; }
        internal string EffectDetail { get; set; }
        public static PolicyDefinition Parse(string content)
        {
            PolicyDefinition policyDefinition = new PolicyDefinition();
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement.GetProperty("properties");
            if (!root.TryGetProperty("policyRule", out JsonElement ruleE))
                throw new Exception("cannot find policyRule node");
            policyDefinition.PolicyRule.If = ruleE.GetProperty("if").GetRawText();
            policyDefinition.PolicyRule.Then = ruleE.GetProperty("then").GetRawText();

            return policyDefinition;
        }
    }
}
