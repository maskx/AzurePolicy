using maskx.AzurePolicy.Utilities;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace maskx.AzurePolicy.Definitions
{
    /// <summary>
    /// <see cref="https://docs.microsoft.com/en-us/azure/governance/policy/concepts/definition-structure#type"/>
    /// <seealso cref=""/>
    /// </summary>
    public class PolicyRule
    {
        [JsonConverter(typeof(RawTextConverter))]
        public string If { get; set; }
        public Effect Then { get; set; }
        public static implicit operator PolicyRule(string rawString)
        {
            return JsonSerializer.Deserialize<PolicyRule>(rawString, SerializerOptions.Default);
        }
    }
}
