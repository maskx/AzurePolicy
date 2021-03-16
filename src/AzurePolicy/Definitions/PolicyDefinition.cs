using maskx.AzurePolicy.Utilities;
using System.Text.Json;
using System.Text.Json.Serialization;

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
        public PolicyTypeEnum PolicyType { get; set; }
        public string Mode { get; set; }
        [JsonConverter(typeof(RawTextConverter))]
        public string Metadata { get; set; }
        [JsonConverter(typeof(RawTextConverter))]
        public string Parameters { get; set; }
        public PolicyRule PolicyRule { get; set; }
        public static implicit operator PolicyDefinition(string rawString)
        {
            return JsonSerializer.Deserialize<PolicyDefinition>(rawString,SerializerOptions.Default);
        }
        public PolicyDefinition() { }
    }
}
